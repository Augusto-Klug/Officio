using ContaFacil.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace ContaFacil.Infrastructure.Services;

public class D4SignOptions
{
    public string BaseUrl { get; set; } = default!;
    public string TokenApi { get; set; } = default!;
    public string CryptoKey { get; set; } = default!;
    public string SafeId { get; set; } = default!;
}

public class D4SignService(
    HttpClient httpClient,
    IOptions<D4SignOptions> options,
    ILogger<D4SignService> logger) : ISignatureService
{
    public async Task<SignatureResult> SendForSignatureAsync(
        SignatureRequest request,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Upload do PDF para D4Sign
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(request.PdfStream), "file", request.DocumentName);

            var uploadResponse = await httpClient.PostAsync(
                $"documents/{options.Value.SafeId}/upload", content, ct);
            uploadResponse.EnsureSuccessStatusCode();

            var uploadResult = await uploadResponse.Content
                .ReadFromJsonAsync<D4SignUploadResponse>(ct);

            // 2. Adiciona signatários
            foreach (var signer in request.Signers)
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

            var links = request.Signers
                .Select(s => new SignerLink(s.Email, $"https://app.d4sign.com.br/sign/{uploadResult.Uuid}"))
                .ToList();

            return SignatureResult.Success(uploadResult.Uuid, links);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "D4Sign indisponível. Ativando fallback manual.");
            return SignatureResult.Failure(
                "Assinatura digital temporariamente indisponível. " +
                "O contrato foi gerado em PDF para assinatura manual.");
        }
    }

    public async Task<SignatureStatusResult> GetStatusAsync(string envelopeId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<D4SignStatusResponse>(
                $"documents/{envelopeId}", ct);

            var status = response!.StatusId switch
            {
                "1" => "processing",
                "2" => "awaiting_signatures",
                "3" => "signed",
                "4" => "canceled",
                _ => "unknown"
            };

            return new SignatureStatusResult(status, response.SignedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar status do envelope {EnvelopeId}", envelopeId);
            throw;
        }
    }
}

internal record D4SignUploadResponse(string Uuid);
internal record D4SignStatusResponse(string StatusId, DateTime? SignedAt);
