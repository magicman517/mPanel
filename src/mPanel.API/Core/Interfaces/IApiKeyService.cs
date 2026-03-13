namespace mPanel.API.Core.Interfaces;

internal sealed record ApiKeyResult
{
    public required string Key { get; init; }
    public required string Prefix { get; init; }
    public required string Hash { get; init; }
}

internal interface IApiKeyService
{
    ApiKeyResult GenerateKey();
    string? GetPrefix(string rawKey);
    string HashKey(string key);
    bool ValidateKey(string rawKey, string storedHash);
}