using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Extensions;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.Nodes;

internal sealed class NodeHeartbeatEndpoint(ILogger<NodeHeartbeatEndpoint> logger, PanelDbContext dbContext)
    : Endpoint<NodeHeartbeatRequest>
{
    public override void Configure()
    {
        Post("/nodes/heartbeat");
        AuthSchemes(AppAuthSchemes.NodeToken);
        Description(d =>
        {
            d.WithTags("Nodes");
            d.Produces(200);
        });
    }

    public override async Task HandleAsync(NodeHeartbeatRequest req, CancellationToken ct)
    {
        if (User.GetNodeId() is not { } nodeId)
        {
            logger.LogWarning("Received heartbeat request with invalid node token");
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var rowsAffected = await dbContext.Nodes
            .Where(x => x.Id == nodeId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.LastHeartbeat, DateTime.UtcNow)
                .SetProperty(x => x.LastHeartbeatCpuUsage, req.CpuUsage)
                .SetProperty(x => x.LastHeartbeatMemoryMb, req.MemoryUsageMb)
                .SetProperty(x => x.LastHeartbeatDiskMb, req.DiskUsageMb), ct);

        if (rowsAffected == 0)
        {
            logger.LogWarning("Received heartbeat, but failed to update record in db for node {Id}", nodeId);
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
        logger.LogInformation("Heartbeat updated for node {Id}", nodeId);
    }
}

[UsedImplicitly]
internal sealed record NodeHeartbeatRequest
{
    public required double CpuUsage { get; init; }
    public required ulong MemoryUsageMb { get; init; }
    public required ulong DiskUsageMb { get; init; }
}