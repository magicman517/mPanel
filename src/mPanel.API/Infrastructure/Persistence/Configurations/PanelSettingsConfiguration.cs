using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mPanel.API.Core.Entities;

namespace mPanel.API.Infrastructure.Persistence.Configurations;

public class PanelSettingsConfiguration : IEntityTypeConfiguration<PanelSettings>
{
    public void Configure(EntityTypeBuilder<PanelSettings> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
    }
}