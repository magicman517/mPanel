using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Extensions;
using mPanel.API.Features.ApiKeys.Shared;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.ApiKeys;

internal sealed class GetApiKeysEndpoint(PanelDbContext dbContext) : EndpointWithoutRequest<IReadOnlyCollection<ApiKeyDto>>
{
    public override void Configure()
    {
        Get("/api-keys");
        AuthSchemes(AppAuthSchemes.Cookie, AppAuthSchemes.ApiKey);
        Description(d =>
        {
            d.WithTags("API Keys");
            d.Produces<IReadOnlyCollection<ApiKeyDto>>();
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var apiKeys = await dbContext.ApiKeys
            .Where(k => k.UserId == userId)
            .Select(k => new ApiKeyDto
            {
                Id = k.Id,
                Name = k.Name,
                Prefix = k.Prefix,
                ExpiresAt = k.ExpiresAt
            })
            .ToListAsync(ct);

        await Send.OkAsync(apiKeys, ct);
    }
}