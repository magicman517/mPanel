using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Entities;
using mPanel.API.Infrastructure.Jobs.Commands;

namespace mPanel.API.Features.Auth;

[UsedImplicitly]
internal sealed record SignInRequest
{
    public required string Identity { get; init; }
    public required string Password { get; init; }
}

internal sealed class SignInRequestValidator : Validator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Identity)
            .NotEmpty().WithMessage("Email or Username is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

internal sealed class SignInEndpoint(
    ILogger<SignInEndpoint> logger,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : Endpoint<SignInRequest>
{
    public override void Configure()
    {
        Post("/auth/sign-in");
        AllowAnonymous();
        Description(d =>
        {
            d.WithTags("Auth");
            d.Produces(200);
        });
    }

    public override async Task HandleAsync(SignInRequest req, CancellationToken ct)
    {
        var user = req.Identity.Contains('@')
            ? await userManager.FindByEmailAsync(req.Identity)
            : await userManager.FindByNameAsync(req.Identity);

        if (user is null)
        {
            ThrowError("Invalid credentials");
        }

        var wasAlreadyLockedOut = await userManager.IsLockedOutAsync(user);
        var result = await signInManager.PasswordSignInAsync(user, req.Password, isPersistent: true, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed sign-in attempt for user {Identity}. Reason: {Reason}", req.Identity, result.ToString());

            if (result.IsLockedOut && !wasAlreadyLockedOut)
            {
                var lockoutEnd = await userManager.GetLockoutEndDateAsync(user);
                var lockoutMinutes = lockoutEnd.HasValue
                    ? (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                    : 5;

                await new SendAccountLockedJob
                {
                    Recipient = user.Email!,
                    LockoutMinutes = Math.Max(1, lockoutMinutes)
                }.QueueJobAsync(ct: ct);
            }

            ThrowError("Invalid credentials");
        }

        await Send.OkAsync(cancellation: ct);
    }
}