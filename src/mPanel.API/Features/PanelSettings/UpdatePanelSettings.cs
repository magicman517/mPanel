using FastEndpoints;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using mPanel.API.Core.Constants;
using mPanel.API.Core.Entities;
using mPanel.API.Infrastructure.Persistence;

namespace mPanel.API.Features.PanelSettings;

[UsedImplicitly]
internal sealed record UpdatePanelSettingsRequest
{
    public required string Name { get; init; }
    public Uri? Url { get; init; }

    public bool AllowRegistration { get; init; }
    public bool AllowAccountSelfDeletion { get; init; }

    public required Smtp Smtp { get; init; }
}

internal sealed class UpdatePanelSettingsRequestValidator : Validator<UpdatePanelSettingsRequest>
{
    public UpdatePanelSettingsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(32).WithMessage("Name must be at most 32 characters long");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Panel URL cannot be empty")
            .When(x => x.Url is not null);

        RuleFor(x => x.Smtp.Port)
            .GreaterThan(0).WithMessage("SMTP Port must be greater than 0")
            .LessThan(65536).WithMessage("SMTP Port must be less than 65536");
    }
}

internal sealed class UpdatePanelSettingsEndpoint(PanelDbContext dbContext)
    : Endpoint<UpdatePanelSettingsRequest>
{
    public override void Configure()
    {
        Put("/settings");
        AuthSchemes(AppAuthSchemes.Cookie, AppAuthSchemes.ApiKey);
        Roles("Admin");
        Description(d =>
        {
            d.WithTags("Settings");
            d.Produces(204);
        });
    }

    public override async Task HandleAsync(UpdatePanelSettingsRequest req, CancellationToken ct)
    {
        var settings = await dbContext.PanelSettings.FirstAsync(ct);

        settings.Name = req.Name;
        settings.Url = req.Url?.ToString();
        settings.AllowRegistration = req.AllowRegistration;
        settings.AllowAccountSelfDeletion = req.AllowAccountSelfDeletion;
        settings.Smtp = req.Smtp;

        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}