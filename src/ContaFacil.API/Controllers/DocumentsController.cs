using ContaFacil.Application.Features.Documents.Commands.UploadDocument;
using ContaFacil.Application.Features.Documents.Commands.ValidateDocument;
using ContaFacil.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaFacil.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController(IMediator mediator) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        IFormFile arquivo,
        [FromForm] DocumentType tipo,
        [FromForm] Guid? clientId,
        [FromForm] Guid? prospectId,
        [FromForm] Guid? socioId,
        [FromForm] string? observacoes,
        CancellationToken ct)
    {
        await using var stream = arquivo.OpenReadStream();
        var command = new UploadDocumentCommand(stream, arquivo.FileName, tipo, clientId, prospectId, socioId, observacoes);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(null, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/validate")]
    public async Task<IActionResult> Validate(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ValidateDocumentCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
