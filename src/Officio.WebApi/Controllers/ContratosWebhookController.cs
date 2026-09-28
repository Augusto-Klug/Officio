using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.WebApi.Servicos.Webhooks;

namespace Officio.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/contratos/webhook/autentique")]
public class ContratosWebhookController(
    IFilaWebhookAutentique fila,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Receber(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        if (!AssinaturaValida(payload, Request.Headers["x-autentique-signature"].FirstOrDefault()))
        {
            MetricasWebhookAutentique.RegistrarRecebido("invalid_signature");
            return Unauthorized();
        }

        if (!fila.TentarEnfileirar(payload, Activity.Current?.Context ?? default))
        {
            MetricasWebhookAutentique.RegistrarRecebido("queue_full");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        MetricasWebhookAutentique.RegistrarRecebido("accepted");
        return Accepted();
    }

    private bool AssinaturaValida(string payload, string? assinaturaRecebida)
    {
        var segredo = configuration["Autentique:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(segredo) || string.IsNullOrWhiteSpace(assinaturaRecebida))
            return false;

        var assinaturaNormalizada = assinaturaRecebida.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
            ? assinaturaRecebida["sha256=".Length..]
            : assinaturaRecebida;

        var chave = Encoding.UTF8.GetBytes(segredo);
        var bytesPayload = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(chave, bytesPayload);
        var esperado = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(esperado),
            Encoding.ASCII.GetBytes(assinaturaNormalizada.ToLowerInvariant()));
    }
}
