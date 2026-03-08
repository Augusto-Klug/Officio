using ContaFacil.Application.Common;
using MediatR;

namespace ContaFacil.Application.Features.Documents.Commands.ValidateDocument;

public record ValidateDocumentCommand(Guid DocumentId) : IRequest<Result<ValidationResultDto>>;
