using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Jobs.Commands;

public sealed record SendPasswordChangedJob : ICommand
{
    public required string Recipient { get; init; }
}

[UsedImplicitly]
internal sealed class SendPasswordChangedHandler(
    IDbContextFactory<PanelDbContext> dbContextFactory,
    IEmailService emailService,
    IEmailTemplateRenderer templateRenderer)
    : ICommandHandler<SendPasswordChangedJob>
{
    public async Task ExecuteAsync(SendPasswordChangedJob command, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var settings = await dbContext.PanelSettings.FirstAsync(ct);

        var emailBody = await templateRenderer.RenderAsync("password-changed", new
        {
            PanelName = settings.Name
        }, ct);

        await emailService.SendEmailAsync(command.Recipient, $"{settings.Name} — Password Changed", emailBody,
            isBodyHtml: true, ct);
    }
}
