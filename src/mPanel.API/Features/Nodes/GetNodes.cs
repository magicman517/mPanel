using System.ComponentModel;
using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Common;
using mPanel.API.Core.Constants;
using mPanel.API.Features.Nodes.Shared;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.Nodes;

internal sealed class GetNodesEndpoint(PanelDbContext dbContext) : Endpoint<GetNodesRequest, PagedResponse<NodeDto>>
{
    public override void Configure()
    {
        Get("/nodes");
        Roles("Admin");
        AuthSchemes(AppAuthSchemes.Cookie, AppAuthSchemes.ApiKey);
        Description(d =>
        {
            d.WithTags("Nodes");
            d.Produces<PagedResponse<NodeDto>>();
        });
    }

    public override async Task HandleAsync(GetNodesRequest req, CancellationToken ct)
    {
        var query = dbContext.Nodes;

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(n => n.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(NodeDto.FromEntity)
            .ToListAsync(ct);

        await Send.OkAsync(new PagedResponse<NodeDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = req.Page,
            PageSize = req.PageSize,
        }, ct);
    }
}

[UsedImplicitly]
internal sealed record GetNodesRequest
{
    [QueryParam]
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    [QueryParam]
    [DefaultValue(20)]
    public int PageSize { get; init; } = 20;
}