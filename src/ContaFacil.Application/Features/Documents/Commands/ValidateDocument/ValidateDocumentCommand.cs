using ContaFacil.Application.Common;
using MediatR;

namespace ContaFacil.Application.Features.Documents.Commands.ValidateDocument;

public record ValidateDocumentCommand(
    Guid DocumentId) : IRequest<Result<ValidationResultDto>>;

public record ValidationResultDto(
    bool Valido,
    decimal ConfiancaOcr,
    List<DivergenciaDto> Divergencias,
    bool FallbackManual,
    string? Message);

public record DivergenciaDto(
    string Campo,
    string ValorDocumento,
    string ValorCadastro,
    decimal Similaridade);
