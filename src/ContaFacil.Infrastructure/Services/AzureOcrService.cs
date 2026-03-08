using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ContaFacil.Infrastructure.Services;

public class AzureOcrService(
    DocumentAnalysisClient azureClient,
    ILogger<AzureOcrService> logger) : IOcrService
{
    public async Task<OcrResult> AnalyzeDocumentAsync(
        Stream documentStream,
        DocumentType type,
        CancellationToken ct = default)
    {
        var modelId = type switch
        {
            DocumentType.CartaoCnpj => "prebuilt-document",
            DocumentType.RgSocio => "prebuilt-idDocument",
            DocumentType.CnhSocio => "prebuilt-idDocument",
            DocumentType.ContratoSocial => "prebuilt-document",
            _ => "prebuilt-document"
        };

        try
        {
            var operation = await azureClient
                .AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    modelId,
                    documentStream,
                    cancellationToken: ct);

            var result = operation.Value;

            var fields = result.Documents
                .FirstOrDefault()?.Fields
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Content ?? string.Empty)
                ?? [];

            var confidence = result.Documents
                .FirstOrDefault()?.Confidence ?? 0;

            return OcrResult.Success(fields, (decimal)confidence);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar OCR para {DocumentType}", type);
            return OcrResult.Failure("Serviço de OCR indisponível.");
        }
    }
}
