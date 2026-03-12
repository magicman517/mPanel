using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.PanelSettings;

internal sealed record GetPublicSettingsResponse
{
    public required string Name { get; init; }

    public bool AllowRegistration { get; init; }
    public bool AllowAccountSelfDeletion { get; init; }
}

internal sealed class GetPublicSettingsEndpoint(PanelDbContext dbContext) : EndpointWithoutRequest<GetPublicSettingsResponse>
{
    public override void Configure()
    {
        Get("/settings/public");
        AllowAnonymous();
        ResponseCache(10);
        Description(d =>
        {
            d.WithTags("Settings");
            d.Produces<GetPublicSettingsResponse>();
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var settings = await dbContext.PanelSettings
            .AsNoTracking()
            .FirstAsync(ct);

        await Send.OkAsync(new GetPublicSettingsResponse
        {
            Name = settings.Name,
            AllowRegistration = settings.AllowRegistration,
            AllowAccountSelfDeletion = settings.AllowAccountSelfDeletion
        }, ct);
    }
}