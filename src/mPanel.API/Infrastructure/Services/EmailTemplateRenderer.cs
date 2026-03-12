using System.Collections.Concurrent;
using System.Reflection;
using mPanel.API.Core.Interfaces;
using Scriban;
using Scriban.Runtime;

namespace mPanel.API.Infrastructure.Services;

internal sealed class EmailTemplateRenderer(ILogger<EmailTemplateRenderer> logger) : IEmailTemplateRenderer
{
    private static readonly ConcurrentDictionary<string, Template> Cache = new();
    private static readonly Assembly Assembly = typeof(EmailTemplateRenderer).Assembly;

    public async Task<string> RenderAsync(string templateName, object model, CancellationToken ct = default)
    {
        var template = Cache.GetOrAdd(templateName, LoadTemplate);

        var scriptObject = new ScriptObject();
        scriptObject.Import(model);

        var context = new TemplateContext { StrictVariables = true };
        context.PushGlobal(scriptObject);

        var result = await template.RenderAsync(context);

        logger.LogDebug("Rendered email template '{TemplateName}'", templateName);

        return result;
    }

    private static Template LoadTemplate(string templateName)
    {
        var resourceName = $"mPanel.API.Infrastructure.Templates.Email.{templateName}.html";

        using var stream = Assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException(
                               $"Email template '{templateName}' not found. " +
                               $"Expected embedded resource: '{resourceName}'. " +
                               $"Ensure the file exists and is marked as EmbeddedResource in the .csproj.");

        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        var template = Template.Parse(text);

        if (template.HasErrors)
            throw new InvalidOperationException(
                $"Email template '{templateName}' has parse errors:{Environment.NewLine}" +
                string.Join(Environment.NewLine, template.Messages));

        return template;
    }
}
