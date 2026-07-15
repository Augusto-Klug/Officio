using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.GerarPdfContrato;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;

namespace Officio.WebApi.Controladores;

[ApiController]
[Controller]
[Route("api/contratos")]
[Authorize]
public class ContratosControlador(IMediator mediator) : ControllerBase
{
    [HttpPost("{id:guid}/generate-pdf")]
    public async Task<IActionResult> GeneratePdf(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GerarPdfContratoComando(id), ct);
        return result.Sucesso ? Ok(result.Valor) : BadRequest(result.Erro);
    }

    [HttpPost("{id:guid}/send-for-signature")]
    public async Task<IActionResult> EnviarParaAssinatura(
        Guid id,
        [FromBody] SendForSignatureRequest request,
        CancellationToken ct)
    {
        var command = new EnviarParaAssinaturaComando(id, request.Signatarios, request.MensagemPersonalizada);
        var result = await mediator.Send(command, ct);
        return result.Sucesso ? Ok(result.Valor) : BadRequest(result.Erro);
    }
}

public record SendForSignatureRequest(
    List<SignatarioDto> Signatarios,
    string? MensagemPersonalizada = null);






