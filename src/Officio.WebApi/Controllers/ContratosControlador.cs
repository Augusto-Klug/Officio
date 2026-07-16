using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.GerarPdfContrato;
using Officio.ViewModels.Contratos;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/contratos/[action]")]
[Authorize]
public class ContratosControlador(IMediator mediator) : ControllerBase
{
    [HttpPost("{id:guid}")]
    public async Task<IActionResult> GerarPdf(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new GerarPdfContratoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> EnviarParaAssinatura(
        Guid id,
        [FromBody] EnviarAssinaturaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }
}
