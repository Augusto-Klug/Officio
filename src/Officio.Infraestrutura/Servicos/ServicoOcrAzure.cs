using Azure;
using Azure.AI.DocumentIntelligence;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Enumeradores;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
                .Concat(result.Documents
                    .Where(documento => !string.IsNullOrWhiteSpace(documento.DocumentType))
                    .Select(documento => new CampoOcrBruto(
                        "__tipo_azure",
                        documento.DocumentType!,
                        Convert.ToDecimal(documento.Confidence))))
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
                fields.Confiancas,
                ObterRequestId(operation.GetRawResponse()) ?? operation.GetRawResponse()?.ClientRequestId,
                operation.GetRawResponse()?.Status);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ConverterFalha(ex, type, modelId);
        }
    }

    private ResultadoOcr ConverterFalha(Exception exception, TipoDocumento tipo, string modelId)
    {
        var falha = Desembrulhar(exception);
        if (falha is RequestFailedException requestFailed)
        {
            var response = requestFailed.GetRawResponse();
            var causaInterna = Desembrulhar(requestFailed.InnerException ?? requestFailed);
            var category = requestFailed.Status <= 0
                ? causaInterna is TimeoutException or TaskCanceledException ? "TIMEOUT" : "FALHA_REDE"
                : ClassificarFalha(requestFailed.Status);
            var requestId = ObterRequestId(response) ?? response?.ClientRequestId;
            var message = CriarMensagemFalha(category);
            var diagnostic = CriarDiagnosticoTecnico(exception, requestFailed.Status, requestFailed.ErrorCode, requestId);
            logger.LogWarning(
                exception,
                "Falha OCR Azure. Categoria={Categoria} StatusHttp={StatusHttp} Codigo={Codigo} RequestId={RequestId} Modelo={Modelo} TipoCausa={TipoCausa}",
                category, requestFailed.Status, requestFailed.ErrorCode, requestId, modelId, causaInterna.GetType().Name);
            return ResultadoOcr.CriarFalha(
                message,
                DescreverModelo(modelId),
                category,
                requestFailed.ErrorCode,
                requestFailed.Status,
                requestId,
                diagnostic);
        }

        if (falha is HttpRequestException)
        {
            var diagnostic = CriarDiagnosticoTecnico(exception);
            logger.LogWarning(
                exception,
                "Falha de rede ao consultar OCR Azure. Modelo={Modelo} Diagnostico={Diagnostico}",
                modelId, diagnostic);
            return ResultadoOcr.CriarFalha(
                CriarMensagemFalha("FALHA_REDE"),
                DescreverModelo(modelId),
                "FALHA_REDE",
                diagnosticoTecnico: diagnostic);
        }

        if (falha is TimeoutException or TaskCanceledException)
        {
            var diagnostic = CriarDiagnosticoTecnico(exception);
            logger.LogWarning(exception, "Timeout ao consultar OCR Azure. Modelo={Modelo} Diagnostico={Diagnostico}", modelId, diagnostic);
            return ResultadoOcr.CriarFalha(
                CriarMensagemFalha("TIMEOUT"),
                DescreverModelo(modelId),
                "TIMEOUT",
                diagnosticoTecnico: diagnostic);
        }

        var diagnosticoInesperado = CriarDiagnosticoTecnico(exception);
        logger.LogWarning(
            exception,
            "Erro inesperado no OCR Azure. TipoDocumento={TipoDocumento} Modelo={Modelo} Diagnostico={Diagnostico}",
            tipo, modelId, diagnosticoInesperado);
        return ResultadoOcr.CriarFalha(
            CriarMensagemFalha("FALHA_INTEGRACAO"),
            DescreverModelo(modelId),
            "FALHA_INTEGRACAO",
            diagnosticoTecnico: diagnosticoInesperado);
    }

    private static string CriarDiagnosticoTecnico(
        Exception exception,
        int? statusHttp = null,
        string? codigoErro = null,
        string? requestId = null)
    {
        var causas = new List<string>();
        for (var atual = exception; atual is not null && causas.Count < 8; atual = atual.InnerException)
            causas.Add($"{atual.GetType().FullName}: {atual.Message}");

        return JsonSerializer.Serialize(new
        {
            excecoes = causas,
            statusHttp,
            codigoErro,
            requestId
        });
    }

    internal static Exception Desembrulhar(Exception exception)
    {
        if (exception is not AggregateException aggregate)
            return exception;

        var excecoes = aggregate.Flatten().InnerExceptions;
        return excecoes.OfType<RequestFailedException>().FirstOrDefault()
            ?? excecoes.OfType<HttpRequestException>().FirstOrDefault()
            ?? excecoes.OfType<TimeoutException>().FirstOrDefault()
            ?? excecoes.OfType<TaskCanceledException>().FirstOrDefault()
            ?? excecoes.FirstOrDefault()
            ?? exception;
    }

    internal static string ClassificarFalha(int statusHttp)
        => statusHttp <= 0 ? "FALHA_REDE" : statusHttp switch
        {
            401 or 403 => "CREDENCIAIS_INVALIDAS",
            408 => "TIMEOUT",
            413 or 415 => "DOCUMENTO_REJEITADO",
            429 => "LIMITE_REQUISICOES",
            >= 500 => "AZURE_INDISPONIVEL",
            >= 400 => "REQUISICAO_REJEITADA",
            _ => "FALHA_INTEGRACAO"
        };

    private static string CriarMensagemFalha(string categoria)
        => categoria switch
        {
            "CREDENCIAIS_INVALIDAS" => "A validacao automatica esta temporariamente indisponivel. A contabilidade pode conferir o documento manualmente.",
            "LIMITE_REQUISICOES" => "A validacao automatica esta ocupada. Aguarde um pouco e tente novamente.",
            "DOCUMENTO_REJEITADO" => "Nao foi possivel abrir este arquivo. Confira se esta legivel e envie-o novamente.",
            "TIMEOUT" => "A validacao esta demorando mais que o esperado. Tente novamente em instantes.",
            "AZURE_INDISPONIVEL" => "A validacao automatica esta temporariamente indisponivel. Tente novamente mais tarde.",
            "FALHA_REDE" => "Nao foi possivel concluir a validacao agora. Tente novamente em instantes ou solicite uma conferencia manual.",
            _ => "Nao foi possivel concluir a validacao automatica. Tente novamente ou solicite uma conferencia manual."
        };

    private static string? ObterRequestId(Response? response)
    {
        if (response is null)
            return null;

        if (!string.IsNullOrWhiteSpace(response.Headers.RequestId))
            return response.Headers.RequestId;

        foreach (var cabecalho in new[] { "apim-request-id", "x-ms-request-id", "x-ms-client-request-id" })
        {
            if (response.Headers.TryGetValue(cabecalho, out var valor) && !string.IsNullOrWhiteSpace(valor))
                return valor;
        }

        return response.ClientRequestId;
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





