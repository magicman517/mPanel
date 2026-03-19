using mPanel.API.Core.Interfaces;

namespace mPanel.API.Infrastructure.Services;

internal sealed class NodeTokenService : INodeTokenService
{
    private const int PrefixLength = 8;

    public NodeTokenResult GenerateToken()
    {
        var result = TokenGenerator.Generate(string.Empty, PrefixLength);
        return new NodeTokenResult { Token = result.Value, Hash = result.Hash, Prefix = result.Prefix };
    }

    public string? GetPrefix(string rawToken) => TokenGenerator.GetPrefix(rawToken, PrefixLength);

    public string HashToken(string token) => TokenGenerator.Hash(token);

    public bool ValidateToken(string rawToken, string storedHash) =>
        TokenGenerator.Validate(rawToken, storedHash);
}
