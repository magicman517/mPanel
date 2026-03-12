using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.Users;

internal sealed record GetCurrentUserResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string AvatarUrl { get; init; }

    public required ICollection<string> Roles { get; init; }

    public bool HasPassword { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool TwoFactorEnabled { get; init; }

    public DateTime CreatedAt { get; init; }
}

internal sealed class GetCurrentUserEndpoint(UserManager<ApplicationUser> userManager) : EndpointWithoutRequest<GetCurrentUserResponse>
{
    public override void Configure()
    {
        Get("/users/@me");
        Description(d =>
        {
            d.WithTags("Users");
            d.Produces<GetCurrentUserResponse>();
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var roles = await userManager.GetRolesAsync(user);
        var hasPassword = await userManager.HasPasswordAsync(user);

        await Send.OkAsync(new GetCurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Username = user.UserName!,
            AvatarUrl = user.AvatarUrl,
            Roles = roles,
            HasPassword = hasPassword,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
        }, ct);
    }
}