using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Jobs.Commands;

public sealed record SendEmailVerificationJob : ICommand
{
    public required Guid UserId { get; init; }
    public required string Recipient { get; init; }
    public required string Token { get; init; }
}

[UsedImplicitly]
internal sealed class SendEmailVerificationHandler(
    ILogger<SendEmailVerificationHandler> logger,
    IDbContextFactory<PanelDbContext> dbContextFactory,
    IEmailService emailService,
    IEmailTemplateRenderer templateRenderer)
    : ICommandHandler<SendEmailVerificationJob>
{
    public async Task ExecuteAsync(SendEmailVerificationJob command, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var settings = await dbContext.PanelSettings.FirstAsync(ct);

        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            logger.LogWarning("Panel URL is not configured. Cannot send email verification to {Recipient}",
                command.Recipient);
            return;
        }

        var encodedToken = Uri.EscapeDataString(command.Token);
        var verificationLink = $"{settings.Url.TrimEnd('/')}/api/auth/verify-email?userId={command.UserId}&token={encodedToken}";

        var emailBody = await templateRenderer.RenderAsync("verification-email", new
        {
            PanelName = settings.Name,
            VerificationLink = verificationLink
        }, ct);

        await emailService.SendEmailAsync(command.Recipient, $"{settings.Name} — Email Verification", emailBody,
            isBodyHtml: true, ct);
    }
}