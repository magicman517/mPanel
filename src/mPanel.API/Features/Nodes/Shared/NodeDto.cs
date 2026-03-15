using System.Linq.Expressions;
using mPanel.API.Core.Entities;

namespace mPanel.API.Features.Nodes.Shared;

internal sealed record NodeDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    public required string TokenPrefix { get; init; }

    public required string Address { get; init; }
    public required int Port { get; init; }
    public string? Alias { get; init; }

    public required int SftpPort { get; init; }
    public string? SftpAlias { get; init; }

    public ulong? MaxMemoryMb { get; init; }
    public ulong? MaxDiskMb { get; init; }

    public string? OsName { get; set; }
    public string? Architecture { get; set; }
    public int? CpuCores { get; set; }
    public ulong? TotalMemoryMb { get; set; }
    public ulong? TotalDiskMb { get; set; }

    public bool IsMaintenanceMode { get; set; }
    public bool IsActive { get; set; }

    public string? HandshakeError { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DateTime? LastHeartbeat { get; set; }
    public double? LastHeartbeatCpuUsage { get; set; }
    public ulong? LastHeartbeatMemoryMb { get; set; }
    public ulong? LastHeartbeatDiskMb { get; set; }

    public static readonly Expression<Func<Node, NodeDto>> FromEntity =
        entity => new NodeDto
        {
            Id = entity.Id,
            Name = entity.Name,
            TokenPrefix = entity.TokenPrefix,
            Address = entity.Address,
            Port = entity.Port,
            Alias = entity.Alias,
            SftpPort = entity.SftpPort,
            SftpAlias = entity.SftpAlias,
            MaxMemoryMb = entity.MaxMemoryMb,
            MaxDiskMb = entity.MaxDiskMb,
            OsName = entity.OsName,
            Architecture = entity.Architecture,
            CpuCores = entity.CpuCores,
            TotalMemoryMb = entity.TotalMemoryMb,
            TotalDiskMb = entity.TotalDiskMb,
            IsMaintenanceMode = entity.IsMaintenanceMode,
            IsActive = entity.IsActive,
            HandshakeError = entity.HandshakeError,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            LastHeartbeat = entity.LastHeartbeat,
            LastHeartbeatCpuUsage = entity.LastHeartbeatCpuUsage,
            LastHeartbeatMemoryMb = entity.LastHeartbeatMemoryMb,
            LastHeartbeatDiskMb = entity.LastHeartbeatDiskMb,
        };
}