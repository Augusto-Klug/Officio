using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.ValidarDocumento;
using Officio.ViewModels.Documentos;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/documentos/[action]")]
[Authorize]
public class DocumentosControlador(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Enviar(
        IFormFile arquivo,
        [FromForm] EnviarDocumentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (arquivo.Length == 0)
            return BadRequest("Arquivo não informado.");

        await using var stream = arquivo.OpenReadStream();
        var resultado = await mediator.Send(
            viewModel.ParaComando(stream, arquivo.FileName),
            cancellationToken);

        return resultado.Sucesso
            ? CreatedAtAction(nameof(Enviar), resultado.Valor)
            : BadRequest(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Validar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ValidarDocumentoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }
}
