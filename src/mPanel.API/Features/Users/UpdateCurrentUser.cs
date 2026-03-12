using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;
using mPanel.API.Extensions;
using mPanel.API.Infrastructure.Jobs.Commands;

namespace mPanel.API.Features.Users;

[UsedImplicitly]
internal sealed record UpdateCurrentUserRequest
{
    public required string Email { get; init; }
    public required string Username { get; init; }
}

internal sealed class UpdateCurrentUserRequestValidator : Validator<UpdateCurrentUserRequest>
{
    public UpdateCurrentUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(64).WithMessage("Username must be at most 64 characters long")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username must only contain alphanumeric characters, dots, underscores, or hyphens");
    }
}

internal sealed class UpdateCurrentUserEndpoint(
    ILogger<UpdateCurrentUserEndpoint> logger,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Endpoint<UpdateCurrentUserRequest>
{
    public override void Configure()
    {
        Put("/users/@me");
        AuthSchemes(AppAuthSchemes.Cookie);
        Options(x => x.RequireRateLimiting("ProfileUpdate"));
        Description(d =>
        {
            d.WithTags("Users");
            d.Produces(200);
        });
    }

    public override async Task HandleAsync(UpdateCurrentUserRequest req, CancellationToken ct)
    {
        if (User.GetUserId() is not { } userId)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (!user.EmailConfirmed)
        {
            ThrowError("Email must be verified to update profile", 403);
        }

        var requiresSignInRefresh = false;

        if (user.UserName != req.Username)
        {
            var result = await userManager.SetUserNameAsync(user, req.Username);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    AddError(error.Description);
                }

                ThrowIfAnyErrors();
            }

            requiresSignInRefresh = true;
        }

        if (user.Email != req.Email)
        {
            var existingEmail = await userManager.FindByEmailAsync(req.Email);
            if (existingEmail is not null)
            {
                ThrowError("Email is already in use", 409);
            }

            var token = await userManager.GenerateChangeEmailTokenAsync(user, req.Email);
            await new SendEmailChangeJob
            {
                UserId = user.Id,
                NewEmail = req.Email,
                Token = token
            }.QueueJobAsync(ct: ct);
        }

        if (requiresSignInRefresh)
        {
            await signInManager.RefreshSignInAsync(user);
        }

        logger.LogInformation(
            "Processed update request for user ID {UserId}. Requested Email: {RequestedEmail}, Requested Username: {RequestedUsername}",
            user.Id, req.Email, req.Username);

        await Send.OkAsync(cancellation: ct);
    }
}