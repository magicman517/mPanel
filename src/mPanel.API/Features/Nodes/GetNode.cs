using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Features.Nodes.Shared;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.Nodes;

internal sealed class GetNodeEndpoint(PanelDbContext dbContext) : Endpoint<GetNodeRequest, NodeDto>
{
    public override void Configure()
    {
        Get("/nodes/{id:guid}");
        AuthSchemes(AppAuthSchemes.Cookie, AppAuthSchemes.ApiKey);
        Roles("Admin");
        Description(d =>
        {
            d.WithTags("Nodes");
            d.Produces<NodeDto>();
        });
    }

    public override async Task HandleAsync(GetNodeRequest req, CancellationToken ct)
    {
        var node = await dbContext.Nodes
            .Select(NodeDto.FromEntity)
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (node is null)
        {
            ThrowError("Node not found", 404);
        }

        await Send.OkAsync(node, ct);
    }
}

[UsedImplicitly]
internal sealed record GetNodeRequest
{
    [RouteParam] public required Guid Id { get; init; }
}