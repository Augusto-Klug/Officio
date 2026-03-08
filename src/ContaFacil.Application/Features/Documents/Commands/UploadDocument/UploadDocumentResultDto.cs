namespace ContaFacil.Application.Features.Documents.Commands.UploadDocument;

public record UploadDocumentResultDto(
    Guid Id,
    string NomeArquivo,
    decimal QualidadeEstimada,
    bool Legivel,
    string Message);
