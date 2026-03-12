using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Jobs.Commands;

public sealed record SendAccountLockedJob : ICommand
{
    public required string Recipient { get; init; }
    public required int LockoutMinutes { get; init; }
}

[UsedImplicitly]
internal sealed class SendAccountLockedHandler(
    IDbContextFactory<PanelDbContext> dbContextFactory,
    IEmailService emailService,
    IEmailTemplateRenderer templateRenderer)
    : ICommandHandler<SendAccountLockedJob>
{
    public async Task ExecuteAsync(SendAccountLockedJob command, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var settings = await dbContext.PanelSettings.FirstAsync(ct);

        var emailBody = await templateRenderer.RenderAsync("account-locked", new
        {
            PanelName = settings.Name,
            command.LockoutMinutes
        }, ct);

        await emailService.SendEmailAsync(command.Recipient, $"{settings.Name} — Account Locked", emailBody,
            isBodyHtml: true, ct);
    }
}
