using mPanel.API.Core.Interfaces;

namespace mPanel.API.Infrastructure.Services;

internal sealed class ApiKeyService : IApiKeyService
{
    private const int PrefixLength = 11; // "sk_" + 8 chars
    private const string EnvironmentPrefix = "sk_";

    public ApiKeyResult GenerateKey()
    {
        var result = TokenGenerator.Generate(EnvironmentPrefix, PrefixLength);
        return new ApiKeyResult { Key = result.Value, Hash = result.Hash, Prefix = result.Prefix };
    }

    public string? GetPrefix(string rawKey) => TokenGenerator.GetPrefix(rawKey, PrefixLength);

    public string HashKey(string key) => TokenGenerator.Hash(key);

    public bool ValidateKey(string rawKey, string storedHash) =>
        TokenGenerator.Validate(rawKey, storedHash);
}
