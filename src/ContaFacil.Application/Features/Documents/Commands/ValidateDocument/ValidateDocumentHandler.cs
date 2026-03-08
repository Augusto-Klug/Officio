using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Features.Documents.Commands.ValidateDocument;

public class ValidateDocumentHandler(
    DbContext db,
    IOcrService ocrService,
    IStorageService storage) : IRequestHandler<ValidateDocumentCommand, Result<ValidationResultDto>>
{
    public async Task<Result<ValidationResultDto>> Handle(
        ValidateDocumentCommand request,
        CancellationToken ct)
    {
        var document = await db.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, ct);

        if (document is null)
            return Result<ValidationResultDto>.Failure("Documento não encontrado.");

        if (document.QualidadeEstimada < 0.4m)
            return Result<ValidationResultDto>.Failure(
                "Qualidade do documento abaixo do limite mínimo. Reenvie uma imagem mais nítida.");

        await using var stream = await storage.DownloadAsync(document.CaminhoStorage, ct);

        var ocrResult = await ocrService.AnalyzeDocumentAsync(stream, document.Tipo, ct);

        if (!ocrResult.Sucesso)
        {
            var fallback = DocumentValidation.RequireManualReview(document.Id);
            db.Set<DocumentValidation>().Add(fallback);
            await db.SaveChangesAsync(ct);
            return Result<ValidationResultDto>.Success(
                new ValidationResultDto(false, 0, [], true, "OCR indisponível. Documento marcado para revisão manual."));
        }

        var divergencias = CompareFields(ocrResult.CamposExtraidos, new Dictionary<string, string>());
        var valido = divergencias.Count == 0 || divergencias.All(d => d.Similaridade >= 0.85m);

        var validation = DocumentValidation.Create(
            document.Id,
            ocrResult.CamposExtraidos,
            divergencias.Cast<object>().ToList(),
            valido,
            ocrResult.ConfiancaOcr);

        db.Set<DocumentValidation>().Add(validation);
        await db.SaveChangesAsync(ct);

        return Result<ValidationResultDto>.Success(
            new ValidationResultDto(valido, ocrResult.ConfiancaOcr, divergencias, false, null));
    }

    private static List<DivergenciaDto> CompareFields(
        Dictionary<string, string> ocr,
        Dictionary<string, string> cadastro)
    {
        var divergencias = new List<DivergenciaDto>();

        foreach (var (campo, valorCadastro) in cadastro)
        {
            if (!ocr.TryGetValue(campo, out var valorOcr)) continue;

            var similaridade = CalculateSimilarity(
                Normalize(valorOcr),
                Normalize(valorCadastro));

            if (similaridade < 0.85m)
                divergencias.Add(new DivergenciaDto(campo, valorOcr, valorCadastro, similaridade));
        }

        return divergencias;
    }

    private static decimal CalculateSimilarity(string a, string b)
    {
        if (a == b) return 1m;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0m;

        var maxLen = Math.Max(a.Length, b.Length);
        var distance = LevenshteinDistance(a, b);
        return 1m - (decimal)distance / maxLen;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var matrix = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++) matrix[i, 0] = i;
        for (var j = 0; j <= b.Length; j++) matrix[0, j] = j;

        for (var i = 1; i <= a.Length; i++)
        for (var j = 1; j <= b.Length; j++)
        {
            var cost = a[i - 1] == b[j - 1] ? 0 : 1;
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + cost);
        }

        return matrix[a.Length, b.Length];
    }

    private static string Normalize(string s)
        => s.ToUpperInvariant().Trim()
            .Replace(".", "").Replace("-", "").Replace("/", "");
}
