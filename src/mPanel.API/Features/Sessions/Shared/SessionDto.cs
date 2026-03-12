namespace mPanel.API.Features.Sessions.Shared;

internal sealed record SessionDto
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public required string Username { get; init; }

    public required IEnumerable<string> Roles { get; init; }
}