using ContaFacil.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace ContaFacil.Infrastructure.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        // TODO: integrar com SendGrid, SMTP ou outro provedor
        logger.LogInformation("Enviando e-mail para {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
