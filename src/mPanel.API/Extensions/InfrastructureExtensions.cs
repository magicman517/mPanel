using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Entities;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Identity;
using mPanel.API.Infrastructure.Persistence;
using mPanel.API.Infrastructure.Persistence.Converters;
using mPanel.API.Infrastructure.Services;
using StackExchange.Redis;
using StackExchange.Redis.KeyspaceIsolation;

namespace mPanel.API.Extensions;

public static class InfrastructureExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddInfrastructure()
        {
            var pgConnectionString = builder.Configuration.GetConnectionString("mPanel")
                                     ?? builder.Configuration.GetConnectionString("postgres");

            var redisConnectionString = builder.Configuration.GetConnectionString("redis");

            builder.Services.AddDbContextPool<PanelDbContext>(options =>
            {
                options.UseNpgsql(pgConnectionString);
                options.UseAsyncSeeding(DatabaseExtensions.SeedAsync);
            });

            builder.Services.AddPooledDbContextFactory<PanelDbContext>(options =>
            {
                options.UseNpgsql(pgConnectionString);
            });

            builder.Services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 0;

                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<ApplicationRole>()
                .AddSignInManager()
                .AddEntityFrameworkStores<PanelDbContext>()
                .AddDefaultTokenProviders();

            builder.Services
                .AddDataProtection()
                .SetApplicationName("mPanel")
                .PersistKeysToDbContext<PanelDbContext>();

            builder.Services.AddSingleton<EncryptedConverter>(sp =>
            {
                var protector = sp
                    .GetRequiredService<IDataProtectionProvider>()
                    .CreateProtector("mPanel.SensitiveData");
                return new EncryptedConverter(protector);
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnectionString!));

            builder.Services.AddSingleton<IDatabase>(sp =>
            {
                var connection = sp.GetRequiredService<IConnectionMultiplexer>();
                return connection.GetDatabase().WithKeyPrefix("mPanel:");
            });

            builder.Services.AddSingleton<RedisTicketStore>();

            builder.Services.AddSingleton<IEmailService, EmailService>();
            builder.Services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
            builder.Services.AddSingleton<IApiKeyService, ApiKeyService>();
            builder.Services.AddSingleton<INodeTokenService, NodeTokenService>();

            return builder;
        }
    }
}