using System.Net;
using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Enums;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.Nodes;

internal sealed class UpdateNodeEndpoint(PanelDbContext dbContext) : Endpoint<UpdateNodeRequest>
{
    public override void Configure()
    {
        Put("/nodes/{id:guid}");
        AuthSchemes(AppAuthSchemes.Cookie, AppAuthSchemes.ApiKey);
        Roles(AppRoles.Admin);
        Description(d =>
        {
            d.WithTags("Nodes");
            d.Produces(204);
        });
    }

    public override async Task HandleAsync(UpdateNodeRequest req, CancellationToken ct)
    {
        var node = await dbContext.Nodes.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
        if (node is null)
        {
            ThrowError("Node not found", 404);
        }

        var existingName = await dbContext.Nodes.FirstOrDefaultAsync(x => x.Name == req.Name && x.Id != req.Id, ct);
        if (existingName is not null)
        {
            ThrowError($"Name {req.Name} is already taken", 409);
        }

        var address = req.Address
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');

        node.Name = req.Name;
        node.Scheme = req.Scheme;
        node.Address = address;
        node.Port = req.Port;
        node.Alias = req.Alias;
        node.SftpPort = req.SftpPort;
        node.SftpAlias = req.SftpAlias;
        node.MaxMemoryMb = req.MaxMemoryMb;
        node.MaxDiskMb = req.MaxDiskMb;
        node.IsMaintenanceMode = req.IsMaintenanceMode;
        node.IsActive = req.IsActive;

        await dbContext.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}

[UsedImplicitly]
internal sealed record UpdateNodeRequest
{
    [RouteParam] public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required NodeConnectionScheme Scheme { get; init; }
    public required string Address { get; init; }
    public int Port { get; init; } = 10001;
    public string? Alias { get; init; }
    public int SftpPort { get; init; } = 2022;
    public string? SftpAlias { get; init; }
    public ulong? MaxMemoryMb { get; init; }
    public ulong? MaxDiskMb { get; init; }
    public bool IsMaintenanceMode { get; init; }
    public bool IsActive { get; init; } = true;
}

internal sealed class UpdateNodeRequestValidator : Validator<UpdateNodeRequest>
{
    public UpdateNodeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name is too short")
            .MaximumLength(128).WithMessage("Name is too long");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .Must(BeAValidIpOrHostName).WithMessage("Address must be a valid IP address or hostname");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535");

        RuleFor(x => x.Alias)
            .MaximumLength(128).WithMessage("Alias is too long")
            .When(x => !string.IsNullOrWhiteSpace(x.Alias));

        RuleFor(x => x.SftpPort)
            .InclusiveBetween(1, 65535).WithMessage("SFTP Port must be between 1 and 65535")
            .NotEqual(x => x.Port).WithMessage("SFTP Port must be different from the main port");

        RuleFor(x => x.SftpAlias)
            .MaximumLength(128).WithMessage("SFTP Alias is too long")
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
