using System.Security.Claims;
using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.Users;

[UsedImplicitly]
internal sealed record UpdateCurrentUserPasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
    public required string ConfirmNewPassword { get; init; }
}

internal sealed class UpdateCurrentUserPasswordRequestValidator : Validator<UpdateCurrentUserPasswordRequest>
{
    public UpdateCurrentUserPasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters long")
            .MaximumLength(128).WithMessage("New password must be at most 128 characters long")
            .Matches(@"^\S+$").WithMessage("New password must not contain spaces");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm new password is required")
            .Equal(x => x.NewPassword).WithMessage("Confirm new password must match new password");
    }
}

internal sealed class UpdateCurrentUserPasswordEndpoint(
    ILogger<UpdateCurrentUserPasswordEndpoint> logger,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Endpoint<UpdateCurrentUserPasswordRequest>
{
    public override void Configure()
    {
        Put("/users/@me/password");
        Description(d =>
        {
            d.WithTags("Users");
            d.Produces(200);
        });
    }

    public override async Task HandleAsync(UpdateCurrentUserPasswordRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Description);
            }

            ThrowIfAnyErrors();
        }

        await signInManager.RefreshSignInAsync(user);
        await Send.OkAsync(cancellation: ct);

        logger.LogInformation("Password updated for user {UserId}", user.Id);
    }
}