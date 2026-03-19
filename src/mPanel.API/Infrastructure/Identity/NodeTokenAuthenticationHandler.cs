using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Identity;

internal sealed class NodeTokenAuthenticationHandler(
    PanelDbContext dbContext,
    INodeTokenService nodeTokenService,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string HeaderName = "X-Node-Token";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var headerValue))
            return AuthenticateResult.NoResult();

        var rawToken = headerValue.ToString();
        if (nodeTokenService.GetPrefix(rawToken) is not { } prefix)
            return AuthenticateResult.Fail("Invalid token");

        var node = await dbContext.Nodes
            .Where(n => n.TokenPrefix == prefix)
            .FirstOrDefaultAsync();

        if (node is null || !nodeTokenService.ValidateToken(rawToken, node.TokenHash))
            return AuthenticateResult.Fail("Invalid token");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, node.Id.ToString()),
            new(ClaimTypes.Name, node.Name),
            new(ClaimTypes.Role, "Node"),
        };

        var identity = new ClaimsIdentity(claims, AppAuthSchemes.NodeToken);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AppAuthSchemes.NodeToken);

        return AuthenticateResult.Success(ticket);
    }
}