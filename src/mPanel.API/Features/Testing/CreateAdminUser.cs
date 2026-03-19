using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.Testing;

[UsedImplicitly]
internal sealed record CreateAdminUserRequest
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

internal sealed class CreateAdminUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<CreateAdminUserRequest>
{
    public override void Configure()
    {
        Post("/testing/create-admin-user");
        AllowAnonymous();
        Tags("Testing");
    }

    public override async Task HandleAsync(CreateAdminUserRequest req, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Email = req.Email,
            UserName = req.Username,
            EmailConfirmed = true,
            AvatarUrl = ApplicationUser.GenerateAvatarUrl(req.Email)
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create admin user {req.Email}");

        await userManager.AddToRolesAsync(user, [AppRoles.User, AppRoles.Admin]);
        await Send.OkAsync(cancellation: ct);
    }
}