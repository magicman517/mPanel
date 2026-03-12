using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Entities;
using mPanel.API.Infrastructure.Identity;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Extensions;

public static class DatabaseExtensions
{
    extension(IHost app)
    {
        public async Task MigrateDatabaseAsync()
        {
            const long lockId = 192837465;
            string[] roles = ["Admin", "User"];

            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PanelDbContext>();
            var keyManager = scope.ServiceProvider.GetRequiredService<IKeyManager>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            await dbContext.Database.OpenConnectionAsync();

            try
            {
                await dbContext.Database.ExecuteSqlRawAsync($"SELECT pg_advisory_lock({lockId});");

                await dbContext.Database.MigrateAsync();

                var keys = keyManager.GetAllKeys();
                if (!keys.Any(k => !k.IsRevoked && k.ExpirationDate > DateTimeOffset.UtcNow))
                {
                    keyManager.CreateNewKey(
                        activationDate: DateTimeOffset.UtcNow,
                        expirationDate: DateTimeOffset.UtcNow.AddDays(90)
                    );
                }

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new ApplicationRole { Name = role });
                    }
                }
            }
            finally
            {
                await dbContext.Database.ExecuteSqlRawAsync($"SELECT pg_advisory_unlock({lockId});");
                await dbContext.Database.CloseConnectionAsync();
            }
        }
    }

    public static async Task SeedAsync(DbContext dbContext, bool _, CancellationToken ct)
    {
        if (!await dbContext.Set<PanelSettings>().AnyAsync(ct))
        {
            dbContext.Set<PanelSettings>().Add(new PanelSettings());
            await dbContext.SaveChangesAsync(ct);
        }
    }
}