using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.Auth;

[UsedImplicitly]
internal sealed class ChangeEmailRequest
{
    [QueryParam] public Guid UserId { get; init; }
    [QueryParam] public string? NewEmail { get; init; }
    [QueryParam] public string? Token { get; init; }
}

internal sealed class ChangeEmailEndpoint(
    ILogger<ChangeEmailEndpoint> logger,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : Endpoint<ChangeEmailRequest>
{
    private const string SuccessPath = "/auth/change-email/success";
    private const string ErrorPath = "/auth/change-email/error";

    public override void Configure()
    {
        Get("/auth/change-email");
        AuthSchemes(AppAuthSchemes.Cookie);
        Description(d =>
        {
            d.WithTags("Users");
            d.Produces(302);
        });
    }

    public override async Task HandleAsync(ChangeEmailRequest req, CancellationToken ct)
    {
        if (req.UserId == Guid.Empty || string.IsNullOrWhiteSpace(req.NewEmail) || string.IsNullOrWhiteSpace(req.Token))
        {
            await Send.RedirectAsync(ErrorPath);
            return;
        }

        var user = await userManager.FindByIdAsync(req.UserId.ToString());
        if (user is null)
        {
            await Send.RedirectAsync(ErrorPath);
            return;
        }

        var result = await userManager.ChangeEmailAsync(user, req.NewEmail, req.Token);
        if (!result.Succeeded)
        {
            logger.LogWarning("Email change failed for user {UserId}: {Errors}",
                user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            await Send.RedirectAsync(ErrorPath);
            return;
        }

        user.AvatarUrl = ApplicationUser.GenerateAvatarUrl(req.NewEmail);
        await userManager.UpdateAsync(user);
        await signInManager.RefreshSignInAsync(user);

        logger.LogInformation("Email changed for user {UserId}", user.Id);
        await Send.RedirectAsync(SuccessPath);
    }
}
