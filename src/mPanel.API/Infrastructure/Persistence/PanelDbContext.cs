using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Common;
using mPanel.API.Core.Entities;
using mPanel.API.Infrastructure.Jobs;
using mPanel.API.Infrastructure.Persistence.Converters;

namespace mPanel.API.Infrastructure.Persistence;

public sealed class PanelDbContext(DbContextOptions<PanelDbContext> options, EncryptedConverter encryptedConverter)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<JobRecord> JobRecords { get; set; }

    public DbSet<PanelSettings> PanelSettings { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(PanelDbContext).Assembly);

        builder.Entity<PanelSettings>()
            .ComplexProperty(x => x.Smtp, smtp =>
            {
                smtp.Property(s => s.Username).HasConversion(encryptedConverter);
                smtp.Property(s => s.Password).HasConversion(encryptedConverter);
            });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var entities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entity in entities)
        {
            entity.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}