using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Enumeradores;
using Microsoft.Extensions.Logging;

namespace Officio.Infraestrutura.Servicos;

public class ServicoOcrAzure(
    DocumentAnalysisClient azureClient,
    ILogger<ServicoOcrAzure> logger) : IServicoOcr
{
    public async Task<ResultadoOcr> AnalisarDocumentoAsync(
        Stream documentStream,
        TipoDocumento type,
        CancellationToken ct = default)
    {
        var modelId = type switch
        {
            TipoDocumento.CartaoCnpj => "prebuilt-document",
            TipoDocumento.RgSocio => "prebuilt-idDocument",
            TipoDocumento.CnhSocio => "prebuilt-idDocument",
            TipoDocumento.ContratoSocial => "prebuilt-document",
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

            return ResultadoOcr.CriarSucesso(fields, (decimal)confidence);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar OCR para {TipoDocumento}", type);
            return ResultadoOcr.CriarFalha("Serviço de OCR indisponível.");
        }
    }
}





