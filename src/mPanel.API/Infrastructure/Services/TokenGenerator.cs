using System.Security.Cryptography;
using System.Text;

namespace mPanel.API.Infrastructure.Services;

internal static class TokenGenerator
{
    private const int KeyLength = 32;

    public static TokenResult Generate(string prefix, int prefixLength)
    {
        var keyBytes = RandomNumberGenerator.GetBytes(KeyLength);
        var rawKey = prefix + Convert.ToHexString(keyBytes).ToLowerInvariant();

        var extractedPrefix = rawKey[..prefixLength];
        var hash = Hash(rawKey);

        return new TokenResult(rawKey, extractedPrefix, hash);
    }

    public static string? GetPrefix(string rawKey, int prefixLength) =>
        rawKey.Length >= prefixLength ? rawKey[..prefixLength] : null;

    public static string Hash(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool Validate(string rawKey, string storedHash)
    {
        var hash = Hash(rawKey);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(hash),
            Convert.FromHexString(storedHash)
        );
    }
}

internal readonly record struct TokenResult(string Value, string Prefix, string Hash);
