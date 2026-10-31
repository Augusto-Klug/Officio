using System.Text.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Integracoes;
using Officio.Infraestrutura.Integracoes.Autentique;

namespace Officio.Infraestrutura.Servicos;

public class ServicoAutentique(
    IAutentiqueIntegracaoCliente autentiqueIntegracaoCliente,
    IOptions<OpcoesAutentique> options,
    ILogger<ServicoAutentique> logger) : IServicoAssinatura
{
    public async Task<ResultadoBloqueioAssinatura> BloquearDocumentoAsync(string documentoId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiToken))
            return new(false, "Autentique sem ApiToken configurado.");
        try
        {
            var prazo = DateTime.UtcNow;
            var resposta = await autentiqueIntegracaoCliente.AtualizarPrazoAsync(documentoId, prazo, options.Value.ApiToken, ct);
            LogarErrosGraphQl(resposta?.Errors);
            var documento = resposta?.Data?.UpdateDocument;
            if (resposta?.Errors is { Count: > 0 } || documento?.Id != documentoId ||
                documento.DeadlineAt is null || documento.DeadlineAt.Value > DateTimeOffset.UtcNow)
                return new(false, "Nao foi possivel confirmar o bloqueio do documento na Autentique.");
            return new(true);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException || ex is TaskCanceledException && !ct.IsCancellationRequested)
        {
            LogarFalhaIntegracao("BloquearDocumento", ex, 0, documentoId);
            return new(false, "Falha de comunicacao ao bloquear o documento na Autentique.");
        }
    }

    public async Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(
        SolicitacaoAssinatura request,
        CancellationToken ct = default)
    {
        var timer = Stopwatch.StartNew();
        if (string.IsNullOrWhiteSpace(options.Value.ApiToken))
        {
            logger.LogWarning("Autentique sem ApiToken configurado. Ativando fallback manual.");
            return CriarFallbackManual();
        }

        try
        {
            var resultado = await autentiqueIntegracaoCliente.CriarDocumentoAsync(
                request,
                options.Value.ApiToken,
                ct);

            LogarErrosGraphQl(resultado?.Errors);

            var documento = resultado?.Data?.CreateDocument;
            if (documento is null || string.IsNullOrWhiteSpace(documento.Id))
            {
                logger.LogWarning("Autentique retornou resposta sem identificador de documento. Ativando fallback manual.");
                return CriarFallbackManual();
            }

            var links = (documento.Signatures ?? [])
                .Where(assinatura => !string.IsNullOrWhiteSpace(assinatura.Email))
                .Select(assinatura =>
                {
                    var signatario = request.Assinantes.FirstOrDefault(item =>
                        string.Equals(item.Email, assinatura.Email, StringComparison.OrdinalIgnoreCase));
                    return new LinkAssinante(
                        assinatura.Email!,
                        assinatura.Link?.ShortLink ?? CriarLinkAssinatura(assinatura.PublicId ?? documento.Id),
                        signatario?.Nome,
                        signatario?.Tipo);
                })
                .ToList();

            logger.LogInformation(
                "Documento enviado para assinatura Autentique. DocumentoId={DocumentoId} Signatarios={Signatarios} DuracaoMs={DuracaoMs}",
                documento.Id,
                links.Count,
                timer.ElapsedMilliseconds);
            return ResultadoAssinatura.CriarSucesso(documento.Id, links);
        }
        catch (HttpRequestException ex)
        {
            LogarFalhaIntegracao("CriarDocumento", ex, timer.ElapsedMilliseconds);
            return CriarFallbackManual();
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            LogarFalhaIntegracao("CriarDocumento", ex, timer.ElapsedMilliseconds);
            return CriarFallbackManual();
        }
        catch (JsonException ex)
        {
            LogarFalhaIntegracao("CriarDocumento", ex, timer.ElapsedMilliseconds);
            return CriarFallbackManual();
        }
    }

    public async Task<ResultadoDocumentoAssinado> BaixarDocumentoAssinadoAsync(
        string documentoId,
        CancellationToken ct = default)
    {
        var timer = Stopwatch.StartNew();
        if (string.IsNullOrWhiteSpace(options.Value.ApiToken))
            return ResultadoDocumentoAssinado.CriarFalha("Autentique sem ApiToken configurado.");

        try
        {
            var resposta = await autentiqueIntegracaoCliente.ObterDocumentoAsync(
                documentoId,
                options.Value.ApiToken,
                ct);

            LogarErrosGraphQl(resposta?.Errors);

            var documento = resposta?.Data?.Document;
            var urlAssinado = documento?.Files?.Signed;
            if (string.IsNullOrWhiteSpace(urlAssinado))
                return ResultadoDocumentoAssinado.CriarFalha("Autentique nao retornou URL do PDF assinado.");

            var conteudo = await autentiqueIntegracaoCliente.BaixarArquivoAsync(urlAssinado, ct);
            var nomeArquivo = string.IsNullOrWhiteSpace(documento?.Name)
                ? $"{documentoId}-assinado.pdf"
                : $"{documento.Name}-assinado.pdf";

            logger.LogInformation(
                "PDF assinado obtido da Autentique. DocumentoId={DocumentoId} DuracaoMs={DuracaoMs}",
                documentoId,
                timer.ElapsedMilliseconds);
            return ResultadoDocumentoAssinado.CriarSucesso(conteudo, nomeArquivo);
        }
        catch (HttpRequestException ex)
        {
            LogarFalhaIntegracao("BaixarDocumentoAssinado", ex, timer.ElapsedMilliseconds, documentoId);
            return ResultadoDocumentoAssinado.CriarFalha("Falha ao baixar PDF assinado do Autentique.");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            LogarFalhaIntegracao("BaixarDocumentoAssinado", ex, timer.ElapsedMilliseconds, documentoId);
            return ResultadoDocumentoAssinado.CriarFalha("Timeout ao baixar PDF assinado do Autentique.");
        }
        catch (JsonException ex)
        {
            LogarFalhaIntegracao("BaixarDocumentoAssinado", ex, timer.ElapsedMilliseconds, documentoId);
            return ResultadoDocumentoAssinado.CriarFalha("Autentique retornou JSON inesperado ao consultar o documento.");
        }
    }

    public async Task<ResultadoEstadoAssinatura> ConsultarEstadoDocumentoAsync(
        string documentoId,
        CancellationToken ct = default)
    {
        var timer = Stopwatch.StartNew();
        if (string.IsNullOrWhiteSpace(options.Value.ApiToken))
            return ResultadoEstadoAssinatura.CriarFalha("Autentique sem ApiToken configurado.");

        try
        {
            var resposta = await autentiqueIntegracaoCliente.ObterDocumentoAsync(
                documentoId,
                options.Value.ApiToken,
                ct);

            LogarErrosGraphQl(resposta?.Errors);
            if (resposta?.Errors is { Count: > 0 })
                return ResultadoEstadoAssinatura.CriarFalha("Autentique retornou erro ao consultar o documento.");

            var assinaturas = resposta?.Data?.Document?.Signatures;
            if (assinaturas is null || assinaturas.Count == 0)
                return ResultadoEstadoAssinatura.CriarFalha("Autentique nao retornou os signatarios do documento.");

            var signatarios = assinaturas
                .Where(assinatura => !string.IsNullOrWhiteSpace(assinatura.Action?.Name))
                .ToList();
            if (signatarios.Count == 0 || signatarios.Any(s => string.IsNullOrWhiteSpace(s.Email)))
                return ResultadoEstadoAssinatura.CriarFalha("Autentique nao retornou os signatarios do documento.");

            var concluidas = signatarios
                .Select(assinatura => assinatura.Signed?.CreatedAt)
                .ToList();
            var concluido = concluidas.All(data => data.HasValue);
            var concluidoEm = concluido
                ? concluidas.Max()
                : null;

            logger.LogInformation(
                "Estado da assinatura consultado na Autentique. DocumentoId={DocumentoId} Concluido={Concluido} DuracaoMs={DuracaoMs}",
                documentoId,
                concluido,
                timer.ElapsedMilliseconds);
            return ResultadoEstadoAssinatura.CriarSucesso(concluido, concluidoEm, signatarios.Select(s => s.Email!).ToArray());
        }
        catch (HttpRequestException ex)
        {
            LogarFalhaIntegracao("ConsultarEstadoDocumento", ex, timer.ElapsedMilliseconds, documentoId);
            return ResultadoEstadoAssinatura.CriarFalha("Falha de comunicacao ao consultar a Autentique.");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            LogarFalhaIntegracao("ConsultarEstadoDocumento", ex, timer.ElapsedMilliseconds, documentoId);
            return ResultadoEstadoAssinatura.CriarFalha("Timeout ao consultar a Autentique.");
        }
        catch (JsonException ex)
        {
            LogarFalhaIntegracao("ConsultarEstadoDocumento", ex, timer.ElapsedMilliseconds, documentoId);
            return ResultadoEstadoAssinatura.CriarFalha("Autentique retornou resposta inesperada.");
        }
    }

    private static string CriarLinkAssinatura(string publicId)
        => $"https://app.autentique.com.br/assinatura/{publicId}";

    private void LogarErrosGraphQl(List<ErroGraphQlAutentique>? erros)
    {
        if (erros is null || erros.Count == 0)
            return;

        var codigos = erros
            .Select(erro => erro.Extensions is not null &&
                erro.Extensions.TryGetValue("code", out var valor) &&
                valor.ValueKind == JsonValueKind.String
                    ? valor.GetString()
                    : null)
            .Where(codigo => !string.IsNullOrWhiteSpace(codigo))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        logger.LogWarning(
            "Autentique retornou erros GraphQL. Quantidade={Quantidade} Codigos={Codigos}",
            erros.Count,
            string.Join(",", codigos));
    }

    private void LogarFalhaIntegracao(string operacao, Exception exception, long duracaoMs, string? documentoId = null)
        => logger.LogWarning(
            "Falha na integracao Autentique. Operacao={Operacao} DocumentoId={DocumentoId} TipoExcecao={TipoExcecao} StatusHttp={StatusHttp} DuracaoMs={DuracaoMs} StackTrace={StackTrace}",
            operacao,
            documentoId,
            exception.GetType().Name,
            (exception as HttpRequestException)?.StatusCode,
            duracaoMs,
            exception.StackTrace);

    private static ResultadoAssinatura CriarFallbackManual()
        => ResultadoAssinatura.CriarFalha(
            "Assinatura digital temporariamente indisponivel. O contrato foi gerado em PDF para assinatura manual.");
}
