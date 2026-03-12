using System.Security.Claims;
using FastEndpoints;
using mPanel.API.Core.Constants;
using mPanel.API.Features.Sessions.Shared;

namespace mPanel.API.Features.Sessions;

internal sealed class GetCurrentSessionEndpoint : EndpointWithoutRequest<SessionDto>
{
    public override void Configure()
    {
        Get("/sessions/current");
        AuthSchemes(AppAuthSchemes.Cookie);
        Description(d =>
        {
            d.WithTags("Sessions");
            d.Produces<SessionDto>();
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var username = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        await Send.OkAsync(new SessionDto
        {
            Id = userId,
            Email = email,
            Username = username,
            Roles = roles
        }, ct);
    }
}