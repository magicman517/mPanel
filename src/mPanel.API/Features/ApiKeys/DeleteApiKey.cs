using System.Text.Json.Serialization;
using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Extensions;
using mPanel.API.Features.ApiKeys.Shared;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.ApiKeys;

[UsedImplicitly]
internal sealed record DeleteApiKeyRequest
{
    [RouteParam] public required Guid Id { get; init; }
}

internal sealed class DeleteApiKeyEndpoint(ILogger<DeleteApiKeyEndpoint> logger, PanelDbContext dbContext)
    : Endpoint<DeleteApiKeyRequest, IEnumerable<ApiKeyDto>>
{
    public override void Configure()
    {
        Delete("/api-keys/{id:guid}");
        AuthSchemes(AppAuthSchemes.Cookie);
        Description(d =>
        {
            d.WithTags("API Keys");
            d.Produces<IEnumerable<ApiKeyDto>>();
        });
    }

    public override async Task HandleAsync(DeleteApiKeyRequest req, CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var apiKey = await dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Id == req.Id, ct);
        if (apiKey is null)
        {
            ThrowError("API key not found", 404);
        }

        dbContext.ApiKeys.Remove(apiKey);
        await dbContext.SaveChangesAsync(ct);

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

        logger.LogInformation("Deleted API key {ApiKeyId} for user {UserId}", apiKey.Id, userId);
    }
}