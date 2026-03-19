using mPanel.API.Core.Common;
using mPanel.API.Core.Enums;

namespace mPanel.API.Core.Entities;

public class Node : BaseEntity
{
    public required string Name { get; set; }

    public required string TokenPrefix { get; set; }
    public required string TokenHash { get; set; }

    public required NodeConnectionScheme Scheme { get; set; }
    public required string Address { get; set; }
    public required int Port { get; set; }
    public string? Alias { get; set; }

    public required int SftpPort { get; set; }
    public string? SftpAlias { get; set; }

    public ulong? MaxMemoryMb { get; set; }
    public ulong? MaxDiskMb { get; set; }

    public string? OsName { get; set; }
    public string? Architecture { get; set; }
    public int? CpuCores { get; set; }
    public ulong? TotalMemoryMb { get; set; }
    public ulong? TotalDiskMb { get; set; }

    public bool IsMaintenanceMode { get; set; }
    public bool IsActive { get; set; } = true;

    public string? HandshakeError { get; set; }

    public DateTime? LastHeartbeat { get; set; }
    public double? LastHeartbeatCpuUsage { get; set; }
    public ulong? LastHeartbeatMemoryMb { get; set; }
    public ulong? LastHeartbeatDiskMb { get; set; }
}