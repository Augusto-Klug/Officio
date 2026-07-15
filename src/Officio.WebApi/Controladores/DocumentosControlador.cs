using Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;
using Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.ValidarDocumento;
using Officio.Dominio.Enumeradores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Officio.WebApi.Controladores;

[ApiController]
[Controller]
[Route("api/documentos")]
[Authorize]
public class DocumentosControlador(IMediator mediator) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        IFormFile arquivo,
        [FromForm] TipoDocumento tipo,
        [FromForm] Guid? clientId,
        [FromForm] Guid? prospectId,
        [FromForm] Guid? socioId,
        [FromForm] string? observacoes,
        CancellationToken ct)
    {
        await using var stream = arquivo.OpenReadStream();
        var command = new EnviarDocumentoComando(stream, arquivo.FileName, tipo, clientId, prospectId, socioId, observacoes);
        var result = await mediator.Send(command, ct);

        return result.Sucesso
            ? CreatedAtAction(null, result.Valor)
            : BadRequest(result.Erro);
    }

    [HttpPost("{id:guid}/validate")]
    public async Task<IActionResult> Validate(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ValidarDocumentoComando(id), ct);
        return result.Sucesso ? Ok(result.Valor) : BadRequest(result.Erro);
    }
}






