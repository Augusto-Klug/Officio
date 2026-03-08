using ContaFacil.Application.Common;
using ContaFacil.Domain.Enums;
using MediatR;

namespace ContaFacil.Application.Features.Documents.Commands.UploadDocument;

public record UploadDocumentCommand(
    Stream Arquivo,
    string NomeArquivo,
    DocumentType Tipo,
    Guid? ClientId,
    Guid? ProspectId,
    Guid? SocioId,
    string? Observacoes) : IRequest<Result<UploadDocumentResultDto>>;

public record UploadDocumentResultDto(
    Guid Id,
    string NomeArquivo,
    decimal QualidadeEstimada,
    bool Legivel,
    string Message);
