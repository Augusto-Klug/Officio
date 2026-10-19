using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/interno/atualizacoes-tempo-real")]
[RequestSizeLimit(4096)]
public sealed class AtualizacoesInternasController(
    IPublicadorAtualizacaoTempoReal publicador,
    IConfiguration configuracao,
    ILogger<AtualizacoesInternasController> logger) : ControllerBase
{
    public const string HeaderChave = "X-Officio-Internal-Key";

    private static readonly HashSet<string> EntidadesPermitidas =
    [
        EntidadesAtualizacaoTempoReal.Contrato,
        EntidadesAtualizacaoTempoReal.Admissao,
        EntidadesAtualizacaoTempoReal.Prospecto,
        EntidadesAtualizacaoTempoReal.Orcamento,
        EntidadesAtualizacaoTempoReal.CatalogoOrcamento,
        EntidadesAtualizacaoTempoReal.Cliente,
        EntidadesAtualizacaoTempoReal.Notificacao,
        EntidadesAtualizacaoTempoReal.Faturamento,
        EntidadesAtualizacaoTempoReal.Dashboard
    ];

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Publicar(
        [FromBody] EventoAtualizacaoTempoReal? evento,
        CancellationToken cancellationToken)
    {
        if (!ChaveValida(Request.Headers[HeaderChave].FirstOrDefault()))
            return Unauthorized();

        if (evento is null ||
            evento.ParceiroId == Guid.Empty ||
            !EntidadesPermitidas.Contains(evento.Entidade) ||
            evento.Id == Guid.Empty)
            return BadRequest();

        await publicador.PublicarAsync(evento, cancellationToken);
        logger.LogDebug(
            "Callback interno de atualizacao recebido. ParceiroId={ParceiroId} Entidade={Entidade} RegistroId={RegistroId}",
            evento.ParceiroId,
            evento.Entidade,
            evento.Id);
        return NoContent();
    }

    private bool ChaveValida(string? recebida)
    {
        var esperada = configuracao["TempoReal:ChaveCompartilhada"];
        if (string.IsNullOrWhiteSpace(esperada) || string.IsNullOrWhiteSpace(recebida))
            return false;

        var hashEsperado = SHA256.HashData(Encoding.UTF8.GetBytes(esperada));
        var hashRecebido = SHA256.HashData(Encoding.UTF8.GetBytes(recebida));
        return CryptographicOperations.FixedTimeEquals(hashEsperado, hashRecebido);
    }
}
