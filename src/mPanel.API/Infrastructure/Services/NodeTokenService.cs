using System.Security.Cryptography;
using System.Text;
using mPanel.API.Core.Interfaces;

namespace mPanel.API.Infrastructure.Services;

internal sealed class NodeTokenService : INodeTokenService
{
    private const int KeyLength = 32;
    private const int PrefixLength = 8;

    public NodeTokenResult GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(KeyLength);
        var rawToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        var prefix = rawToken[..PrefixLength];
        var hash = HashToken(rawToken);

        return new NodeTokenResult
        {
            Token = rawToken,
            Hash = hash,
            Prefix = prefix
        };
    }

    public string? GetPrefix(string rawToken) =>
        rawToken.Length >= PrefixLength ? rawToken[..PrefixLength] : null;

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool ValidateToken(string rawToken, string storedHash)
    {
        var hash = HashToken(rawToken);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(hash),
            Convert.FromHexString(storedHash)
        );
    }
}