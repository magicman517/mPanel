namespace mPanel.API.Core.Interfaces;

internal sealed record NodeTokenResult
{
    public required string Token { get; init; }
    public required string Prefix { get; init; }
    public required string Hash { get; init; }
}

internal interface INodeTokenService
{
    NodeTokenResult GenerateToken();
    string? GetPrefix(string rawToken);
    string HashToken(string token);
    bool ValidateToken(string rawToken, string storedHash);
}