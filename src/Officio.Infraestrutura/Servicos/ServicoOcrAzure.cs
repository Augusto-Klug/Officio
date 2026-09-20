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
            TipoDocumento.Identidade => "prebuilt-idDocument",
            TipoDocumento.Cpf => "prebuilt-idDocument",
            TipoDocumento.ComprovanteResidencia => "prebuilt-document",
            TipoDocumento.CarteiraTrabalho => "prebuilt-document",
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

            var camposBrutos = result.Documents
                .SelectMany(documento => documento.Fields)
                .Select(campo => new CampoOcrBruto(
                    campo.Key,
                    campo.Value.Content ?? string.Empty,
                    Convert.ToDecimal(campo.Value.Confidence ?? 0f)))
                .Concat(result.KeyValuePairs
                    .Where(par => par.Value is not null)
                    .Select(par => new CampoOcrBruto(
                        par.Key.Content,
                        par.Value!.Content,
                        (decimal)par.Confidence)))
                .ToList();

            var fields = NormalizadorCamposOcr.Normalizar(type, camposBrutos);
            var confidence = result.Documents.Count > 0
                ? result.Documents.Average(documento => documento.Confidence)
                : result.KeyValuePairs.Count > 0
                    ? result.KeyValuePairs.Average(par => par.Confidence)
                    : 0;

            return ResultadoOcr.CriarSucesso(
                fields.Valores,
                (decimal)confidence,
                modelId,
                fields.Confiancas);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar OCR para {TipoDocumento}", type);
            return ResultadoOcr.CriarFalha("Serviço de OCR indisponível.");
        }
    }
}





