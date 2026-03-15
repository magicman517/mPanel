namespace mPanel.API.Features.Nodes.Shared;

internal sealed record NodeHeartbeatDto
{
    public double CpuPercent { get; set; }
    public ulong UsedMemoryMb { get; set; }
    public ulong UsedDiskMb { get; set; }
}