using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.Auth;

[UsedImplicitly]
internal sealed record VerifyEmailRequest
{
    [QueryParam] public Guid UserId { get; init; }
    [QueryParam] public string? Token { get; init; }
}

internal sealed class VerifyEmailEndpoint(
    ILogger<VerifyEmailEndpoint> logger,
    UserManager<ApplicationUser> userManager)
    : Endpoint<VerifyEmailRequest>
{
    private const string SuccessPath = "/auth/verify-email/success";
    private const string ErrorPath = "/auth/verify-email/error";

    public override void Configure()
    {
        Get("/auth/verify-email");
        AllowAnonymous();
        Description(d =>
        {
            d.WithTags("Auth");
            d.Produces(302);
        });
    }

    public override async Task HandleAsync(VerifyEmailRequest req, CancellationToken ct)
    {
        if (req.UserId == Guid.Empty || string.IsNullOrWhiteSpace(req.Token))
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

        if (user.EmailConfirmed)
        {
            await Send.RedirectAsync(SuccessPath);
            return;
        }

        var result = await userManager.ConfirmEmailAsync(user, req.Token);
        if (!result.Succeeded)
        {
            logger.LogWarning("Email verification failed for user {UserId}: {Errors}",
                user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            await Send.RedirectAsync(ErrorPath);
            return;
        }

        logger.LogInformation("Email verified for user {UserId}", user.Id);
        await Send.RedirectAsync(SuccessPath);
    }
}
