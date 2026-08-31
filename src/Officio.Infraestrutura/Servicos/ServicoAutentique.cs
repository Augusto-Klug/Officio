using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Officio.Dominio.Interfaces;
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

            var documento = resultado?.Data?.CreateDocument;
            if (documento is null || string.IsNullOrWhiteSpace(documento.Id))
            {
                logger.LogWarning("Autentique retornou resposta sem identificador de documento. Ativando fallback manual.");
                return CriarFallbackManual();
            }

            var links = (documento.Signatures ?? [])
                .Where(assinatura => !string.IsNullOrWhiteSpace(assinatura.Signer?.Email))
                .Select(assinatura => new LinkAssinante(
                    assinatura.Signer!.Email!,
                    CriarLinkAssinatura(assinatura.PublicId ?? documento.Id)))
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

    private static string CriarLinkAssinatura(string publicId)
        => $"https://app.autentique.com.br/assinatura/{publicId}";

    private static ResultadoAssinatura CriarFallbackManual()
        => ResultadoAssinatura.CriarFalha(
            "Assinatura digital temporariamente indisponivel. O contrato foi gerado em PDF para assinatura manual.");
}
