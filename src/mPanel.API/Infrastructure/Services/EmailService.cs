using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using mPanel.API.Core.Entities;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Services;

internal sealed class EmailService(ILogger<EmailService> logger, IDbContextFactory<PanelDbContext> dbContextFactory)
    : IEmailService
{
    public async Task SendEmailAsync(string recipient, string subject, string body, bool isBodyHtml = false,
        CancellationToken ct = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var settings = (await dbContext.PanelSettings.FirstAsync(ct)).Smtp;

        if (!IsSmtpConfigured(settings, recipient))
            return;

        using var message = BuildMessage(recipient, subject, body, isBodyHtml, settings.From!);
        await SendAsync(message, settings, ct);
        logger.LogInformation("Email sent to {Recipient} with subject '{Subject}'", recipient, subject);
    }

    private bool IsSmtpConfigured(Smtp settings, string recipient)
    {
        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            logger.LogWarning("SMTP host is not configured. Email to {Recipient} will not be sent", recipient);
            return false;
        }

        // ReSharper disable once InvertIf
        if (string.IsNullOrWhiteSpace(settings.From))
        {
            logger.LogWarning("SMTP 'From' address is not configured. Email to {Recipient} will not be sent", recipient);
            return false;
        }

        return true;
    }

    private static MimeMessage BuildMessage(string recipient, string subject, string body, bool isBodyHtml, string from)
    {
        var message = new MimeMessage();

        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = isBodyHtml ? body : null,
            TextBody = isBodyHtml ? null : body
        }.ToMessageBody();

        return message;
    }

    private static SecureSocketOptions ResolveSocketOptions(int port) => port switch
    {
        465 => SecureSocketOptions.SslOnConnect,
        587 or 2525 => SecureSocketOptions.StartTls,
        _ => SecureSocketOptions.Auto
    };

    private static async Task SendAsync(MimeMessage message, Smtp settings, CancellationToken ct)
    {
        using var client = new SmtpClient();
        client.CheckCertificateRevocation = false;

        await client.ConnectAsync(settings.Host, settings.Port, ResolveSocketOptions(settings.Port), ct);

        var hasCredentials = !string.IsNullOrWhiteSpace(settings.Username)
                          && !string.IsNullOrWhiteSpace(settings.Password);

        if (hasCredentials)
            await client.AuthenticateAsync(settings.Username, settings.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
