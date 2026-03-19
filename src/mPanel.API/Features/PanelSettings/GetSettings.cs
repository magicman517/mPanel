using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.PanelSettings;

internal sealed record GetSettingsResponse
{
    public required string Name { get; init; }
    public string? Url { get; init; }

    public bool AllowRegistration { get; init; }
    public bool AllowAccountSelfDeletion { get; init; }

    public required Smtp Smtp { get; init; }
}

internal sealed class GetSettingsEndpoint(PanelDbContext dbContext) : EndpointWithoutRequest<GetSettingsResponse>
{
    public override void Configure()
    {
        Get("/settings");
        AuthSchemes(AppAuthSchemes.Cookie, AppAuthSchemes.ApiKey);
        Roles(AppRoles.Admin);
        Description(d =>
        {
            d.WithTags("Settings");
            d.Produces<GetSettingsResponse>();
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var settings = await dbContext.PanelSettings
            .AsNoTracking()
            .FirstAsync(ct);

        await Send.OkAsync(new GetSettingsResponse
        {
            Name = settings.Name,
            Url = settings.Url,
            AllowRegistration = settings.AllowRegistration,
            AllowAccountSelfDeletion = settings.AllowAccountSelfDeletion,
            Smtp = settings.Smtp
        }, ct);
    }
}