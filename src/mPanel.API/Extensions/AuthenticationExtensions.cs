using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using mPanel.API.Core.Constants;
using mPanel.API.Infrastructure.Identity;

namespace mPanel.API.Extensions;

public static class AuthenticationExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddAuthentication()
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(AppAuthSchemes.ApiKey, null)
                .AddScheme<AuthenticationSchemeOptions, NodeTokenAuthenticationHandler>(AppAuthSchemes.NodeToken, null)
                .AddCookie(IdentityConstants.ApplicationScheme, options =>
                {
                    options.Cookie.Name = "mPanel.Session";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;

                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };
                });

            builder.Services
                .AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
                .PostConfigure<RedisTicketStore>((options, store) => { options.SessionStore = store; });

            builder.Services.AddAuthorization();

            return builder;
        }
    }
}