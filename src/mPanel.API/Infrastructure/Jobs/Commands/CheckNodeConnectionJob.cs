using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Infrastructure.Jobs.Commands;

public sealed record CheckNodeConnectionJob : ICommand
{
    public required Guid NodeId { get; init; }
}

[UsedImplicitly]
internal sealed class CheckNodeConnectionHandler(
    ILogger<CheckNodeConnectionHandler> logger,
    IDbContextFactory<PanelDbContext> dbContextFactory,
    INodeApiService nodeApiService)
    : ICommandHandler<CheckNodeConnectionJob>
{
    public async Task ExecuteAsync(CheckNodeConnectionJob command, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var node = await dbContext.Nodes.FirstOrDefaultAsync(n => n.Id == command.NodeId, ct);
        if (node is null)
        {
            logger.LogWarning("Failed to check node connection: Node {NodeId} not found", command.NodeId);
            return;
        }

        var (isSuccess, errorMessage) = await nodeApiService.CheckHealthAsync(node, ct);
        if (isSuccess)
        {
            if (node.HandshakeError is not null)
            {
                node.HandshakeError = null;
                await dbContext.SaveChangesAsync(ct);
                logger.LogInformation("Node '{NodeName}' connection verified. Handshake error cleared", node.Name);
            }
        }
        else
        {
            node.HandshakeError = errorMessage;
            await dbContext.SaveChangesAsync(ct);
            logger.LogWarning("Node '{NodeName}' connection failed. Saved error to database", node.Name);
        }
    }
}