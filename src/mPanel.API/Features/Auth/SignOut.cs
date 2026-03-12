using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.Auth;

internal sealed class SignOutEndpoint(SignInManager<ApplicationUser> signInManager) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/auth/sign-out");
        AuthSchemes(AppAuthSchemes.Cookie);
        Description(d =>
        {
            d.WithTags("Auth");
            d.Produces(204);
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await signInManager.SignOutAsync();
        await Send.NoContentAsync(ct);
    }
}