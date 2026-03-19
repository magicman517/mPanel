using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Extensions;
using mPanel.API.Infrastructure.Jobs.Commands;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.Nodes;

internal sealed class NodeHandshakeEndpoint(ILogger<NodeHandshakeEndpoint> logger, PanelDbContext dbContext)
    : Endpoint<NodeHandshakeRequest, NodeHandshakeResponse>
{
    public override void Configure()
    {
        Post("/nodes/handshake");
        AuthSchemes(AppAuthSchemes.NodeToken);
        Description(d =>
        {
            d.WithTags("Nodes");
            d.Produces<NodeHandshakeResponse>();
        });
    }

    public override async Task HandleAsync(NodeHandshakeRequest req, CancellationToken ct)
    {
        if (User.GetNodeId() is not { } nodeId)
        {
            logger.LogWarning("Received handshake request with invalid node token");
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var node = await dbContext.Nodes.FirstOrDefaultAsync(x => x.Id == nodeId, ct);
        if (node is null)
        {
            logger.LogWarning("Node {NodeId} authenticated but was not found in database", nodeId);
            await Send.NotFoundAsync(ct);
            return;
        }

        if (req.NodeId is not null && req.NodeId != nodeId)
        {
            logger.LogWarning(
                "Identity conflict! Node {ExpectedId} tried to authenticate with lockfile ID {ReceivedId}", nodeId,
                req.NodeId);

            node.HandshakeError =
                "Identity conflict: The node's local lockfile ID does not match the token provided. Did you reuse a token from another node?";
            await dbContext.SaveChangesAsync(ct);

            await Send.StatusCodeAsync(409, ct);
            return;
        }

        node.OsName = req.OsName;
        node.Architecture = req.Architecture;
        node.CpuCores = req.CpuCores;
        node.TotalMemoryMb = req.TotalMemoryMb;
        node.TotalDiskMb = req.TotalDiskMb;
        await dbContext.SaveChangesAsync(ct);

        // check panel to node connection in 5 seconds
        // to give the node time to finish starting up
        await new CheckNodeConnectionJob
        {
            NodeId = node.Id
        }.QueueJobAsync(DateTime.UtcNow.AddSeconds(5), ct: ct);

        await Send.OkAsync(new NodeHandshakeResponse
        {
            NodeId = node.Id,
            NodeName = node.Name
        }, ct);
    }
}

[UsedImplicitly]
internal sealed record NodeHandshakeRequest
{
    public Guid? NodeId { get; init; }
    public required string OsName { get; init; }
    public required string Architecture { get; init; }
    public required int CpuCores { get; init; }
    public required ulong TotalMemoryMb { get; init; }
    public required ulong TotalDiskMb { get; init; }
}

internal sealed record NodeHandshakeResponse
{
    public required Guid NodeId { get; init; }
    public required string NodeName { get; init; }
}