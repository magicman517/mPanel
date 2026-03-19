namespace mPanel.API.Core.Interfaces;

internal interface IEmailTemplateRenderer
{
    Task<string> RenderAsync(string templateName, object model, CancellationToken ct = default);
}