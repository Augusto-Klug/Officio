using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Features.Documents.Commands.UploadDocument;

public class UploadDocumentHandler(
    DbContext db,
    IStorageService storageService,
    ICurrentTenant currentTenant) : IRequestHandler<UploadDocumentCommand, Result<UploadDocumentResultDto>>
{
    public async Task<Result<UploadDocumentResultDto>> Handle(
        UploadDocumentCommand request,
        CancellationToken ct)
    {
        // TODO: implementar estimativa de qualidade real (ex: análise de imagem)
        var qualidadeEstimada = 0.85m;

        if (qualidadeEstimada < 0.4m)
            return Result<UploadDocumentResultDto>.Failure(
                "Imagem muito escura ou desfocada. Tire uma nova foto com boa iluminação.");

        var path = await storageService.UploadAsync(
            request.Arquivo,
            $"documentos/{Guid.NewGuid()}/{request.NomeArquivo}",
            "documents",
            ct);

        var document = Document.Create(
            currentTenant.TenantId,
            request.NomeArquivo,
            request.Tipo,
            path,
            qualidadeEstimada,
            request.ClientId,
            request.ProspectId,
            request.SocioId,
            request.Observacoes);

        db.Set<Document>().Add(document);
        await db.SaveChangesAsync(ct);

        return Result<UploadDocumentResultDto>.Success(
            new UploadDocumentResultDto(
                document.Id,
                document.NomeArquivo,
                qualidadeEstimada,
                document.Legivel,
                "Documento salvo. Chame /validate para iniciar a validação."));
    }
}
