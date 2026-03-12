namespace mPanel.API.Core.Interfaces;

public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Renders an email template by name with the given model.
    /// Templates are resolved from embedded resources under Infrastructure/Templates/Email/{name}.html.
    /// Model properties are exposed as snake_case variables (e.g. PanelName → panel_name).
    /// </summary>
    Task<string> RenderAsync(string templateName, object model, CancellationToken ct = default);
}
