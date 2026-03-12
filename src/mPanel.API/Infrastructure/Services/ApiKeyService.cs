using System.Security.Cryptography;
using System.Text;
using mPanel.API.Core.Interfaces;

namespace mPanel.API.Infrastructure.Services;

internal sealed class ApiKeyService : IApiKeyService
{
    private const int KeyLength = 32;
    private const int PrefixLength = 11; // "sk_" + 8 chars

    private const string EnvironmentPrefix = "sk_";

    public ApiKeyResult GenerateKey()
    {
        var keyBytes = RandomNumberGenerator.GetBytes(KeyLength);
        var rawKey = EnvironmentPrefix + Convert.ToHexString(keyBytes).ToLowerInvariant();

        var prefix = rawKey[..PrefixLength];
        var hash = HashKey(rawKey);

        return new ApiKeyResult
        {
            Key = rawKey,
            Hash = hash,
            Prefix = prefix
        };
    }

    public string? GetPrefix(string rawKey) =>
        rawKey.Length >= PrefixLength ? rawKey[..PrefixLength] : null;

    public string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool ValidateKey(string rawKey, string storedHash)
    {
        var hash = HashKey(rawKey);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(hash),
            Convert.FromHexString(storedHash)
        );
    }
}