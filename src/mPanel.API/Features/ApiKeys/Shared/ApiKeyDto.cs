namespace mPanel.API.Features.ApiKeys.Shared;

internal sealed record ApiKeyDto
{
    public required Guid Id { get; init; }
    public string? Name { get; init; }
    public required string Prefix { get; init; }
    public DateOnly? ExpiresAt { get; init; }
}