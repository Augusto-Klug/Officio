using ContaFacil.Application.Common;
using MediatR;

namespace ContaFacil.Application.Features.Contracts.Commands.SendForSignature;

public record SendForSignatureCommand(
    Guid ContractId,
    List<SignatarioDto> Signatarios,
    string? MensagemPersonalizada = null) : IRequest<Result<SendForSignatureResultDto>>;
