using System.Text.Json;
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
    public async Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(
        SolicitacaoAssinatura request,
        CancellationToken ct = default)
    {
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
                .Select(assinatura => new LinkAssinante(
                    assinatura.Email!,
                    assinatura.Link?.ShortLink ?? CriarLinkAssinatura(assinatura.PublicId ?? documento.Id)))
                .ToList();

            return ResultadoAssinatura.CriarSucesso(documento.Id, links);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Autentique indisponivel. Ativando fallback manual.");
            return CriarFallbackManual();
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Autentique excedeu o tempo de resposta. Ativando fallback manual.");
            return CriarFallbackManual();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Autentique retornou JSON inesperado. Ativando fallback manual.");
            return CriarFallbackManual();
        }
    }

    public async Task<ResultadoDocumentoAssinado> BaixarDocumentoAssinadoAsync(
        string documentoId,
        CancellationToken ct = default)
    {
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

            return ResultadoDocumentoAssinado.CriarSucesso(conteudo, nomeArquivo);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Falha ao baixar PDF assinado do Autentique para o documento {DocumentoId}.", documentoId);
            return ResultadoDocumentoAssinado.CriarFalha("Falha ao baixar PDF assinado do Autentique.");
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Timeout ao baixar PDF assinado do Autentique para o documento {DocumentoId}.", documentoId);
            return ResultadoDocumentoAssinado.CriarFalha("Timeout ao baixar PDF assinado do Autentique.");
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Autentique retornou JSON inesperado ao consultar documento {DocumentoId}.", documentoId);
            return ResultadoDocumentoAssinado.CriarFalha("Autentique retornou JSON inesperado ao consultar o documento.");
        }
    }

    private static string CriarLinkAssinatura(string publicId)
        => $"https://app.autentique.com.br/assinatura/{publicId}";

    private void LogarErrosGraphQl(List<ErroGraphQlAutentique>? erros)
    {
        if (erros is null || erros.Count == 0)
            return;

        logger.LogWarning(
            "Autentique retornou erros GraphQL: {Erros}",
            JsonSerializer.Serialize(erros, RequisicaoBuilder.JsonOpcoes.Padrao));
    }

    private static ResultadoAssinatura CriarFallbackManual()
        => ResultadoAssinatura.CriarFalha(
            "Assinatura digital temporariamente indisponivel. O contrato foi gerado em PDF para assinatura manual.");
}
