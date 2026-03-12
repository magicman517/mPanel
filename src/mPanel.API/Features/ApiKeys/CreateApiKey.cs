using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;
using mPanel.API.Core.Interfaces;
using mPanel.API.Extensions;
using mPanel.API.Features.ApiKeys.Shared;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.ApiKeys;

[UsedImplicitly]
internal sealed record CreateApiKeyRequest
{
    public string? Name { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

internal sealed class CreateApiKeyRequestValidator : Validator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(128).WithMessage("Name cannot exceed 128 characters")
            .When(x => x.Name is not null);
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt is not null);
    }
}

internal sealed record CreateApiKeyResponse
{
    public required string Key { get; init; }
    public required ApiKeyDto Meta { get; init; }
}

internal sealed class CreateApiKeyEndpoint(
    ILogger<CreateApiKeyEndpoint> logger,
    IApiKeyService apiKeyService,
    PanelDbContext dbContext)
    : Endpoint<CreateApiKeyRequest, CreateApiKeyResponse>
{
    public override void Configure()
    {
        Post("/api-keys");
        AuthSchemes(AppAuthSchemes.Cookie);
        Description(d =>
        {
            d.WithTags("API Keys");
            d.Produces<CreateApiKeyResponse>();
        });
    }

    public override async Task HandleAsync(CreateApiKeyRequest req, CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var totalKeys = await dbContext.ApiKeys.CountAsync(k => k.UserId == userId, ct);
        if (totalKeys >= 25)
        {
            ThrowError(
                "You have reached the maximum number of API keys (25). Please delete existing keys to create new ones");
        }

        var apiKeyResult = apiKeyService.GenerateKey();
        var apiKey = new ApiKey
        {
            Name = req.Name,
            UserId = userId,
            Prefix = apiKeyResult.Prefix,
            Hash = apiKeyResult.Hash,
            ExpiresAt = req.ExpiresAt
        };
        dbContext.ApiKeys.Add(apiKey);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateApiKeyResponse
        {
            Key = apiKeyResult.Key,
            Meta = new ApiKeyDto
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                Prefix = apiKey.Prefix,
                ExpiresAt = apiKey.ExpiresAt
            }
        }, ct);

        logger.LogInformation("Created API Key {KeyId} for user {UserId}", apiKey.Id, userId);
    }
}