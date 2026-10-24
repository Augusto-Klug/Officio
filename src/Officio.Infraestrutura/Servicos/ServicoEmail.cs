using Officio.Dominio.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace Officio.Infraestrutura.Servicos;

public class ServicoEmail(ILogger<ServicoEmail> logger, IConfiguration configuration) : IServicoEmail
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var timer = Stopwatch.StartNew();
        if (configuration.GetValue<bool>("Email:ExporConteudoNoLog"))
        {
            logger.LogWarning("Envio de e-mail suprimido pela configuracao Email:ExporConteudoNoLog.");
            return;
        }

        var host = configuration["Email:Smtp:Host"];
        var remetente = configuration["Email:Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(remetente))
            throw new InvalidOperationException(
                "Configure Email:Smtp:Host e Email:Smtp:From para enviar e-mails fora do desenvolvimento.");

        using var mensagem = new MailMessage(remetente, to, subject, body) { IsBodyHtml = false };
        using var cliente = new SmtpClient(host, configuration.GetValue("Email:Smtp:Port", 587))
        {
            EnableSsl = configuration.GetValue("Email:Smtp:EnableSsl", true)
        };

        var usuario = configuration["Email:Smtp:User"];
        var senha = configuration["Email:Smtp:Password"];
        if (!string.IsNullOrWhiteSpace(usuario))
            cliente.Credentials = new NetworkCredential(usuario, senha);

        logger.LogDebug("Iniciando envio de e-mail por SMTP.");
        try
        {
            await cliente.SendMailAsync(mensagem, ct);
            logger.LogInformation("E-mail enviado por SMTP. DuracaoMs={DuracaoMs}", timer.ElapsedMilliseconds);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning(
                "Falha no envio de e-mail por SMTP. TipoExcecao={TipoExcecao} DuracaoMs={DuracaoMs} StackTrace={StackTrace}",
                exception.GetType().Name,
                timer.ElapsedMilliseconds,
                exception.StackTrace);
            throw;
        }
    }
}





