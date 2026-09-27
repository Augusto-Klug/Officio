using Azure;
using Azure.AI.DocumentIntelligence;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Enumeradores;
using Microsoft.Extensions.Logging;

namespace Officio.Infraestrutura.Servicos;

public class ServicoOcrAzure(
    DocumentIntelligenceClient azureClient,
    ILogger<ServicoOcrAzure> logger) : IServicoOcr
{
    public async Task<ResultadoOcr> AnalisarDocumentoAsync(
        Stream documentStream,
        TipoDocumento type,
        CancellationToken ct = default)
    {
        var modelId = ObterModelo(type);

        try
        {
            var conteudo = await BinaryData.FromStreamAsync(documentStream, ct);
            var opcoes = CriarOpcoes(type, conteudo);

            var operation = await azureClient
                .AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    opcoes,
                    ct);

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
                .Concat(result.Pages.SelectMany((pagina, indicePagina) =>
                {
                    var confiancaPagina = pagina.Words.Count > 0
                        ? Convert.ToDecimal(pagina.Words.Average(palavra => palavra.Confidence))
                        : 0m;

                    return pagina.Lines.Select((linha, indiceLinha) => new CampoOcrBruto(
                        $"__linha_{indicePagina}_{indiceLinha}",
                        linha.Content,
                        confiancaPagina));
                }))
                .ToList();

            var fields = NormalizadorCamposOcr.Normalizar(type, camposBrutos);
            var confidence = result.Documents.Count > 0
                ? result.Documents.Average(documento => documento.Confidence)
                : result.KeyValuePairs.Count > 0
                    ? result.KeyValuePairs.Average(par => par.Confidence)
                    : result.Pages.SelectMany(pagina => pagina.Words).Select(palavra => palavra.Confidence)
                        .DefaultIfEmpty(0f)
                        .Average();

            return ResultadoOcr.CriarSucesso(
                fields.Valores,
                (decimal)confidence,
                DescreverModelo(modelId),
                fields.Confiancas);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OCR Azure indisponivel para {TipoDocumento}", type);
            return ResultadoOcr.CriarFalha(
                "Servico de OCR indisponivel.",
                DescreverModelo(modelId));
        }
    }

    internal static AnalyzeDocumentOptions CriarOpcoes(TipoDocumento tipo, BinaryData conteudo)
    {
        var modelo = ObterModelo(tipo);
        var opcoes = new AnalyzeDocumentOptions(modelo, conteudo);
        if (modelo == "prebuilt-layout")
        {
            opcoes.Locale = "pt-BR";
            opcoes.Features.Add(DocumentAnalysisFeature.KeyValuePairs);
        }

        return opcoes;
    }

    private static string ObterModelo(TipoDocumento tipo)
        => tipo is TipoDocumento.Identidade or TipoDocumento.RgSocio or
            TipoDocumento.Cnh or TipoDocumento.CnhSocio
            ? "prebuilt-idDocument"
            : "prebuilt-layout";

    private static string DescreverModelo(string modelo)
        => modelo == "prebuilt-layout" ? $"{modelo}:keyValuePairs" : modelo;
}





