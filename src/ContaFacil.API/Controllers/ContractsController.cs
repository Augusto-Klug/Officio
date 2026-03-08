using ContaFacil.Application.Features.Contracts.Commands.GenerateContractPdf;
using ContaFacil.Application.Features.Contracts.Commands.SendForSignature;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContaFacil.API.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController(IMediator mediator) : ControllerBase
{
    [HttpPost("{id:guid}/generate-pdf")]
    public async Task<IActionResult> GeneratePdf(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateContractPdfCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/send-for-signature")]
    public async Task<IActionResult> SendForSignature(
        Guid id,
        [FromBody] SendForSignatureRequest request,
        CancellationToken ct)
    {
        var command = new SendForSignatureCommand(id, request.Signatarios, request.MensagemPersonalizada);
        var result = await mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}

public record SendForSignatureRequest(
    List<SignatarioDto> Signatarios,
    string? MensagemPersonalizada = null);
