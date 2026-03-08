using ContaFacil.Application.Common;
using ContaFacil.Application.Features.Prospects.Commands.ConvertToClient;
using ContaFacil.Application.Features.Prospects.Commands.CreateProspect;
using ContaFacil.Application.Features.Prospects.Queries.GetProspects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaFacil.API.Controllers;

[ApiController]
[Route("api/prospects")]
[Authorize]
public class ProspectsController(IMediator mediator) : ControllerBase
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
            new GetProspectsQuery(page, pageSize, status, search, responsavelId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProspectCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(null, new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ConvertProspectCommand(id), ct);

        if (!result.IsSuccess)
            return result.Error!.Contains("não encontrado") ? NotFound(result.Error) : BadRequest(result.Error);

        return CreatedAtAction(null, new
        {
            clientId = result.Value,
            message = "Prospect convertido com sucesso."
        });
    }
}
