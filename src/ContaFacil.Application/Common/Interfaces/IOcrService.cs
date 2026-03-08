using ContaFacil.Application.Common.Models.Ocr;
using ContaFacil.Domain.Enums;

namespace ContaFacil.Application.Common.Interfaces;

public interface IOcrService
{
    Task<OcrResult> AnalyzeDocumentAsync(Stream documentStream, DocumentType type, CancellationToken ct = default);
}
