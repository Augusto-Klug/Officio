using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Interfaces;
using MediatR;

namespace ContaFacil.Application.Features.Contracts.Commands.SendForSignature;

public record SendForSignatureCommand(
    Guid ContractId,
    List<SignatarioDto> Signatarios,
    string? MensagemPersonalizada = null) : IRequest<Result<SendForSignatureResultDto>>;

public record SignatarioDto(string Nome, string Email, string? Cpf, string Tipo);

public record SendForSignatureResultDto(
    string Metodo,
    string? DsignEnvelopeId,
    List<SignerLink>? Signatarios,
    string? Message,
    string? PdfUrl);
