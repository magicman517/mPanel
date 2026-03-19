using System.Linq.Expressions;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.ApiKeys.Shared;

internal sealed record ApiKeyDto
{
    public required Guid Id { get; init; }
    public string? Name { get; init; }
    public required string Prefix { get; init; }
    public DateOnly? ExpiresAt { get; init; }

    public static Expression<Func<ApiKey, ApiKeyDto>> FromEntity => k => new ApiKeyDto
    {
        Id = k.Id,
        Name = k.Name,
        Prefix = k.Prefix,
        ExpiresAt = k.ExpiresAt
    };
}
