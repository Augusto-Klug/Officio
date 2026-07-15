using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Officio.WebApi.Controladores;

[ApiController]
[Controller]
[Route("api/prospectos")]
[Authorize]
public class ProspectosControlador(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] Guid? responsavelId = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new ObterProspectosConsulta(page, pageSize, status, search, responsavelId), ct);
        return result.Sucesso ? Ok(result.Valor) : BadRequest(result.Erro);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CriarProspectoComando command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.Sucesso
            ? CreatedAtAction(null, new { id = result.Valor!.Id }, result.Valor)
            : BadRequest(result.Erro);
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ConverterProspectoComando(id), ct);

        if (!result.Sucesso)
            return result.Erro!.Contains("nÃ£o encontrado") ? NotFound(result.Erro) : BadRequest(result.Erro);

        return CreatedAtAction(null, new
        {
            clientId = result.Valor,
            message = "Prospecto convertido com sucesso."
        });
    }
}






