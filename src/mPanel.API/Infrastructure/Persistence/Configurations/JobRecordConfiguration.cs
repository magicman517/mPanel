using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mPanel.API.Infrastructure.Jobs;

namespace mPanel.API.Infrastructure.Persistence.Configurations;

public class JobRecordConfiguration : IEntityTypeConfiguration<JobRecord>
{
    public void Configure(EntityTypeBuilder<JobRecord> builder)
    {
        builder.HasKey(j => j.TrackingID);

        builder.Property(x => x.Command)
            .HasConversion(
                v => SerializeCommand(v),
                v => DeserializeCommand(v)
            )
            .HasColumnType("jsonb");

        builder.HasIndex(x => new { x.QueueID, x.ExecuteAfter, x.ExpireOn, x.DequeueAfter })
            .HasDatabaseName("IX_JobRecords_QueuePolling")
            .HasFilter("\"IsComplete\" = FALSE AND \"IsCancelled\" = FALSE");
    }

    private static string SerializeCommand(object command)
    {
        var typeName = command.GetType().AssemblyQualifiedName!;
        var data = JsonSerializer.Serialize(command);

        var wrapper = new CommandWrapper { Type = typeName, Data = data };
        return JsonSerializer.Serialize(wrapper);
    }

    private static object DeserializeCommand(string json)
    {
        var wrapper = JsonSerializer.Deserialize<CommandWrapper>(json)!;
        var type = Type.GetType(wrapper.Type)!;
        return JsonSerializer.Deserialize(wrapper.Data, type)!;
    }

    private class CommandWrapper
    {
        public string Type { get; init; } = string.Empty;
        public string Data { get; init; } = string.Empty;
    }
}