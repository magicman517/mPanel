using System.ComponentModel;
using System.Net;
using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;
using mPanel.API.Core.Interfaces;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.Nodes;

internal sealed class CreateNodeEndpoint(
    ILogger<CreateNodeEndpoint> logger,
    INodeTokenService nodeTokenService,
    PanelDbContext dbContext) : Endpoint<CreateNodeRequest, CreateNodeResponse>
{
    private const string DockerImage = "ghcr.io/magicman517/mpanel-node:latest";

    public override void Configure()
    {
        Post("/nodes");
        AuthSchemes(AppAuthSchemes.Cookie, AppAuthSchemes.ApiKey);
        Roles("Admin");
        Description(d =>
        {
            d.WithTags("Nodes");
            d.Produces<CreateNodeResponse>(201);
        });
    }

    public override async Task HandleAsync(CreateNodeRequest req, CancellationToken ct)
    {
        var settings = await dbContext.PanelSettings.FirstAsync(ct);
        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            ThrowError("Panel URL is not configured. Please set the URL in panel settings before adding nodes");
        }

        var existingName = await dbContext.Nodes.FirstOrDefaultAsync(x => x.Name == req.Name, ct);
        if (existingName is not null)
        {
            ThrowError($"Name {req.Name} is already taken", 409);
        }

        var tokenResult = nodeTokenService.GenerateToken();
        var node = new Node
        {
            Name = req.Name,
            TokenPrefix = tokenResult.Prefix,
            TokenHash = tokenResult.Hash,
            Address = req.Address,
            Port = req.Port,
            Alias = req.Alias,
            SftpPort = req.SftpPort,
            SftpAlias = req.SftpAlias,
            MaxMemoryMb = req.MaxMemoryMb,
            MaxDiskMb = req.MaxDiskMb,
            IsMaintenanceMode = req.IsMaintenanceMode,
            IsActive = req.IsActive
        };
        dbContext.Nodes.Add(node);
        await dbContext.SaveChangesAsync(ct);

        await Send.CreatedAtAsync<GetNodeEndpoint>(
            routeValues: new { id = node.Id },
            responseBody: new CreateNodeResponse
            {
                Id = node.Id,
                Token = tokenResult.Token,
                DeployCommand = BuildCommand(settings.Url, tokenResult.Token, node)
            },
            cancellation: ct);

        logger.LogInformation("Created new node with ID {NodeId} and Name {NodeName}", node.Id, node.Name);
    }

    private static string BuildCommand(string panelUrl, string token, Node node)
    {
        return $"docker run -d --name mpanel-node " +
               $"--restart unless-stopped " +
               $"-p {node.Port}:10001 -p {node.SftpPort}:2022 " +
               "-v /var/run/docker.sock:/var/run/docker.sock " +
               "-v /var/lib/mpanel/volumes:/var/lib/mpanel/volumes " +
               "-v /etc/mpanel:/etc/mpanel " +
               $"-e PANEL_URL='{panelUrl}' " +
               $"-e NODE_TOKEN='{token}' " +
               DockerImage;
    }
}

[UsedImplicitly]
internal sealed record CreateNodeRequest
{
    public required string Name { get; init; }

    public required string Address { get; init; }
    [DefaultValue(10001)] public int Port { get; init; } = 10001;
    public string? Alias { get; init; }

    [DefaultValue(2022)] public int SftpPort { get; init; } = 2022;
    public string? SftpAlias { get; init; }

    public ulong? MaxMemoryMb { get; init; }
    public ulong? MaxDiskMb { get; init; }

    [DefaultValue(false)] public bool IsMaintenanceMode { get; init; } = false;
    [DefaultValue(true)] public bool IsActive { get; init; } = true;
}

internal sealed record CreateNodeResponse
{
    public required Guid Id { get; init; }
    public required string Token { get; init; }
    public required string DeployCommand { get; init; }
}

internal sealed class CreateNodeRequestValidator : Validator<CreateNodeRequest>
{
    public CreateNodeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name it too short")
            .MaximumLength(128).WithMessage("Name it too long");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .Must(BeAValidIpOrHostName).WithMessage("Address must be a valid IP address or hostname");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.Alias)
            .MaximumLength(128).WithMessage("Alias it too long")
            .When(x => !string.IsNullOrWhiteSpace(x.Alias));

        RuleFor(x => x.SftpPort)
            .InclusiveBetween(1, 65535).WithMessage("SFTP Port must be between 1 and 65535")
            .NotEqual(x => x.Port).WithMessage("SFTP Port must be different from the main port");

        RuleFor(x => x.SftpAlias)
            .MaximumLength(128).WithMessage("SFTP Alias it too long")
            .When(x => !string.IsNullOrWhiteSpace(x.SftpAlias));

        RuleFor(x => x.MaxMemoryMb)
            .GreaterThan(0ul).WithMessage("Max Memory MB must be greater than 0")
            .When(x => x.MaxMemoryMb.HasValue);

        RuleFor(x => x.MaxDiskMb)
            .GreaterThan(0ul).WithMessage("Max Disk MB must be greater than 0")
            .When(x => x.MaxDiskMb.HasValue);
    }

    private bool BeAValidIpOrHostName(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        if (IPAddress.TryParse(address, out _))
            return true;

        var hostNameType = Uri.CheckHostName(address);
        return hostNameType is UriHostNameType.Dns or UriHostNameType.Basic;
    }
}