namespace mPanel.API.Core.Interfaces;

internal interface IEmailService
{
    Task SendEmailAsync(string recipient, string subject, string body, bool isBodyHtml = false,
        CancellationToken ct = default);
}