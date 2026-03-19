using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;
using mPanel.API.Infrastructure.Jobs.Commands;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.Users;

[UsedImplicitly]
internal sealed record CreateUserRequest
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

internal sealed class CreateUserRequestValidator : Validator<CreateUserRequest>
{
    public CreateUserRequestValidator(IDbContextFactory<PanelDbContext> dbContextFactory)
    {
        RuleFor(x => x)
            .MustAsync(async (_, ct) =>
            {
                await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
                var panelSettings = await dbContext.PanelSettings.FirstAsync(ct);
                return panelSettings.AllowRegistration;
            })
            .OverridePropertyName("generalErrors")
            .WithMessage("Registration is currently disabled");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(64).WithMessage("Username must be at most 64 characters long")
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username must only contain alphanumeric characters, dots, underscores, or hyphens");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MaximumLength(128).WithMessage("Password must be at most 128 characters long")
            .Matches(@"^\S+$").WithMessage("Password must not contain spaces");
    }
}

internal sealed class CreateUserEndpoint(ILogger<CreateUserEndpoint> logger, UserManager<ApplicationUser> userManager)
    : Endpoint<CreateUserRequest>
{
    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("Registration"));
        Description(d =>
        {
            d.WithTags("Users");
            d.Produces(201);
        });
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var isFirstUser = !await userManager.Users.AnyAsync(ct);
        var rolesToAssign = new List<string> { AppRoles.User };
        if (isFirstUser)
        {
            rolesToAssign.Add(AppRoles.Admin);
        }

        var user = new ApplicationUser
        {
            Email = req.Email,
            UserName = req.Username,
            EmailConfirmed = isFirstUser,
            AvatarUrl = ApplicationUser.GenerateAvatarUrl(req.Email)
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Description);
            }

            ThrowIfAnyErrors();
        }

        await userManager.AddToRolesAsync(user, rolesToAssign);

        if (!isFirstUser)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await new SendEmailVerificationJob
            {
                UserId = user.Id,
                Recipient = user.Email!,
                Token = token
            }.QueueJobAsync(ct: ct);
        }

        logger.LogInformation("Created new user with ID {UserId} and email {Email}", user.Id, user.Email);
        logger.LogInformation("Assigned roles {Roles} to user with ID {UserId}", string.Join(", ", rolesToAssign),
            user.Id);

        await Send.CreatedAtAsync<CreateUserEndpoint>(cancellation: ct);
    }
}