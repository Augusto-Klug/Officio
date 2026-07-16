using Officio.Dominio.Interfaces;
using Microsoft.Extensions.Logging;

namespace Officio.Infraestrutura.Servicos;

public class ServicoEmail(ILogger<ServicoEmail> logger) : IServicoEmail
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        // TODO: integrar com SendGrid, SMTP ou outro provedor
        logger.LogInformation("Enviando e-mail para {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}





