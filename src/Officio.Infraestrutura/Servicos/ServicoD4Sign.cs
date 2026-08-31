using Officio.Dominio.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Officio.Infraestrutura.Servicos;

public class OpcoesD4Sign
{
    public string BaseUrl { get; set; } = default!;
    public string TokenApi { get; set; } = default!;
    public string CryptoKey { get; set; } = default!;
    public string SafeId { get; set; } = default!;
}

public class ServicoD4Sign(
    HttpClient httpClient,
    IOptions<OpcoesD4Sign> options,
    ILogger<ServicoD4Sign> logger) : IServicoAssinatura
{
    public async Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(
        SolicitacaoAssinatura request,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Upload do PDF para D4Sign
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(request.FluxoPdf), "file", request.NomeDocumento);

            var uploadResponse = await httpClient.PostAsync(
                $"documents/{options.Value.SafeId}/upload", content, ct);
            uploadResponse.EnsureSuccessStatusCode();

            var uploadResult = await uploadResponse.Content
                .ReadFromJsonAsync<RespostaUploadD4Sign>(ct);

            // 2. Adiciona signatários
            foreach (var signer in request.Assinantes)
            {
                var signerPayload = new
                {
                    email = signer.Email,
                    act = "1",
                    foreign = "0",
                    certificadoicpbr = "0",
                    assinatura_presencial = "0"
                };

                await httpClient.PostAsJsonAsync(
                    $"documents/{uploadResult!.Uuid}/createlist", signerPayload, ct);
            }

            // 3. Envia para assinatura
            await httpClient.PostAsync(
                $"documents/{uploadResult!.Uuid}/sendtosigner", null, ct);

            var links = request.Assinantes
                .Select(s => new LinkAssinante(s.Email, $"https://app.d4sign.com.br/sign/{uploadResult.Uuid}"))
                .ToList();

            return ResultadoAssinatura.CriarSucesso(uploadResult.Uuid, links);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "D4Sign indisponível. Ativando fallback manual.");
            return ResultadoAssinatura.CriarFalha(
                "Assinatura digital temporariamente indisponível. " +
                "O contrato foi gerado em PDF para assinatura manual.");
        }
    }

    public async Task<ResultadoStatusAssinatura> ObterStatusAsync(string envelopeId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<RespostaStatusD4Sign>(
                $"documents/{envelopeId}", ct);

            var status = response!.StatusId switch
            {
                "1" => "processing",
                "2" => "awaiting_signatures",
                "3" => "signed",
                "4" => "canceled",
                _ => "unknown"
            };

            return new ResultadoStatusAssinatura(status, response.AssinadoEm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar status do envelope {EnvelopeId}", envelopeId);
            throw;
        }
    }
}

internal record RespostaUploadD4Sign(string Uuid);
internal record RespostaStatusD4Sign(string StatusId, DateTime? AssinadoEm);





