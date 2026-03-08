using ContaFacil.Application.Common;
using MediatR;

namespace ContaFacil.Application.Features.Contracts.Commands.GenerateContractPdf;

public record GenerateContractPdfCommand(Guid ContractId) : IRequest<Result<ContractPdfResultDto>>;
