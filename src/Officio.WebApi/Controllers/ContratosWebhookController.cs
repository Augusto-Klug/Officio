using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;

namespace Officio.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/contratos/webhook/autentique")]
public class ContratosWebhookController(
    IMediator mediator,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Receber(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        if (!AssinaturaValida(payload, Request.Headers["x-autentique-signature"].FirstOrDefault()))
            return Unauthorized();

        var resultado = await mediator.Send(
            new ProcessarWebhookAutentiqueComando(payload),
            cancellationToken);

        return resultado.Sucesso ? Ok() : BadRequest(resultado.Erro);
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
