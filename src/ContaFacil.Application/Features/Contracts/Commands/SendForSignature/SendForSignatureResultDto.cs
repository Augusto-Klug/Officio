using ContaFacil.Application.Common.Models.Signature;

namespace ContaFacil.Application.Features.Contracts.Commands.SendForSignature;

public record SendForSignatureResultDto(
    string Metodo,
    string? DsignEnvelopeId,
    List<SignerLink>? Signatarios,
    string? Message,
    string? PdfUrl);
