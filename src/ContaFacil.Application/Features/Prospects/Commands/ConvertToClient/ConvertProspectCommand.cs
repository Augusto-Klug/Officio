using ContaFacil.Application.Common;
using MediatR;

namespace ContaFacil.Application.Features.Prospects.Commands.ConvertToClient;

public record ConvertProspectCommand(Guid ProspectId, string? SciCodigo = null) : IRequest<Result<Guid>>;
