using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Jobs.Commands;

public sealed record SendEmailChangeJob : ICommand
{
    public required Guid UserId { get; init; }
    public required string NewEmail { get; init; }
    public required string Token { get; init; }
}

[UsedImplicitly]
internal sealed class SendEmailChangeHandler(
    ILogger<SendEmailChangeHandler> logger,
    IDbContextFactory<PanelDbContext> dbContextFactory,
    IEmailService emailService,
    IEmailTemplateRenderer templateRenderer)
    : ICommandHandler<SendEmailChangeJob>
{
    public async Task ExecuteAsync(SendEmailChangeJob command, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var settings = await dbContext.PanelSettings.FirstAsync(ct);

        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            logger.LogWarning("Panel URL is not configured. Cannot send email change confirmation to {NewEmail}",
                command.NewEmail);
            return;
        }

        var encodedToken = Uri.EscapeDataString(command.Token);
        var encodedEmail = Uri.EscapeDataString(command.NewEmail);
        var changeEmailLink =
            $"{settings.Url.TrimEnd('/')}/api/auth/change-email?userId={command.UserId}&newEmail={encodedEmail}&token={encodedToken}";

        var emailBody = await templateRenderer.RenderAsync("email-change", new
        {
            PanelName = settings.Name,
            ChangeEmailLink = changeEmailLink
        }, ct);

        await emailService.SendEmailAsync(command.NewEmail, $"{settings.Name} — Confirm Email Change", emailBody,
            isBodyHtml: true, ct);
    }
}
