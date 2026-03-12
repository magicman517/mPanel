using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Identity;

internal sealed class ApiKeyAuthenticationHandler(
    PanelDbContext db,
    IApiKeyService apiKeyService,
    UserManager<ApplicationUser> userManager,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string HeaderName = "X-Api-Key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValue))
            return AuthenticateResult.NoResult();

        var rawKey = headerValue.ToString();
        if (apiKeyService.GetPrefix(rawKey) is not { } prefix)
            return AuthenticateResult.Fail("Invalid API key");

        var apiKey = await db.ApiKeys
            .Where(k => k.Prefix == prefix && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
            .FirstOrDefaultAsync();

        if (apiKey is null || !apiKeyService.ValidateKey(rawKey, apiKey.Hash))
            return AuthenticateResult.Fail("Invalid API key");

        var user = await userManager.FindByIdAsync(apiKey.UserId.ToString());
        if (user is null)
            return AuthenticateResult.Fail("Invalid API key");

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, AppAuthSchemes.ApiKey);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AppAuthSchemes.ApiKey);

        return AuthenticateResult.Success(ticket);
    }
}
