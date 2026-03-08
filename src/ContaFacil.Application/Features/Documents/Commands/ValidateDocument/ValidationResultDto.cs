namespace ContaFacil.Application.Features.Documents.Commands.ValidateDocument;

public record ValidationResultDto(
    bool Valido,
    decimal ConfiancaOcr,
    List<DivergenciaDto> Divergencias,
    bool FallbackManual,
    string? Message);
