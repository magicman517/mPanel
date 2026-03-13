using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.HttpOverrides;
using NSwag;
using RedisRateLimiting;
using Scalar.AspNetCore;
using StackExchange.Redis;

namespace mPanel.API.Extensions;

public static class ApiExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddServiceHealthChecks()
        {
            builder.Services.AddServiceHealthChecks(options =>
            {
                options.LivePath = "/api/health/live";
                options.ReadyPath = "/api/health/ready";
            }, checks =>
            {
                var pgConnectionString = builder.Configuration.GetConnectionString("mPanel")
                                         ?? builder.Configuration.GetConnectionString("postgres");
                var redisConnectionString = builder.Configuration.GetConnectionString("redis");

                if (!string.IsNullOrEmpty(pgConnectionString))
                    checks.AddNpgSql(pgConnectionString, name: "postgres");

                if (!string.IsNullOrEmpty(redisConnectionString))
                    checks.AddRedis(redisConnectionString, name: "redis");
            });

            return builder;
        }

        public WebApplicationBuilder AddOpenApi()
        {
            builder.Services.SwaggerDocument(options =>
            {
                options.EnableJWTBearerAuth = false;
                options.AutoTagPathSegmentIndex = 0;
                options.DocumentSettings = settings =>
                {
                    settings.Title = "mPanel API";
                    settings.Version = "v1";

                    settings.AddAuth("Cookie", new OpenApiSecurityScheme
                    {
                        Name = "mPanel.Session",
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        In = OpenApiSecurityApiKeyLocation.Cookie
                    });

                    settings.AddAuth("Api Key", new OpenApiSecurityScheme
                    {
                        Name = "X-API-Key",
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        In = OpenApiSecurityApiKeyLocation.Header
                    });
                };
            });

            return builder;
        }

        public WebApplicationBuilder AddRateLimiting()
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("Global", context =>
                {
                    var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    var partitionKey = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                       ?? context.Connection.RemoteIpAddress?.ToString()
                                       ?? "anonymous";
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                        $"global:{partitionKey}",
                        _ => new RedisSlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            ConnectionMultiplexerFactory = () => redis
                        });
                });

                options.AddPolicy("Registration", context =>
                {
                    var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                        $"registration:{partitionKey}",
                        _ => new RedisSlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            ConnectionMultiplexerFactory = () => redis
                        });
                });

                options.AddPolicy("ProfileUpdate", context =>
                {
                    var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    // user id has to be in claims because auth is required for this endpoint
                    var partitionKey = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                        $"profile_update:{partitionKey}",
                        _ => new RedisSlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 1,
                            Window = TimeSpan.FromMinutes(5),
                            ConnectionMultiplexerFactory = () => redis
                        });
                });

                options.AddPolicy("PasswordUpdate", context =>
                {
                    var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                    // user id has to be in claims because auth is required for this endpoint
                    var partitionKey = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                        $"password_update:{partitionKey}",
                        _ => new RedisSlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromHours(1),
                            ConnectionMultiplexerFactory = () => redis
                        });
                });
            });

            return builder;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseOpenApi()
        {
            app.UseOpenApi(options => { options.Path = "/api/openapi/{documentName}.json"; });

            app.MapScalarApiReference("/api/reference", options =>
            {
                options
                    .WithOpenApiRoutePattern("/api/openapi/{documentName}.json")
                    .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.OFetch)
                    .HideDocumentDownload()
                    .HideDeveloperTools()
                    .HideClientButton()
                    .HideModels()
                    .DisableAgent()
                    .DisableTelemetry();

                if (!app.Environment.IsDevelopment())
                {
                    options.HideTestRequestButton();
                }
            });

            return app;
        }
    }
}
