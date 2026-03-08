using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Application.Common.Models.Ocr;
using ContaFacil.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ContaFacil.Infrastructure.Services
{
    /// <summary>
    /// Fallback para quando as credenciais do Azure OCR não estão configuradas.
    /// Retorna sempre um resultado indicando que a revisão manual é necessária,
    /// sem lançar exceção — o handler trata normalmente pelo campo Sucesso = false.
    /// </summary>
    public class NullOcrService(ILogger<NullOcrService> logger) : IOcrService
    {
        public Task<OcrResult> AnalyzeDocumentAsync(
            Stream documentStream,
            DocumentType type,
            CancellationToken ct = default)
        {
            logger.LogWarning(
                "OCR indisponível (NullOcrService ativo). " +
                "Configure AzureOcr:Endpoint e AzureOcr:Key no appsettings. " +
                "Documento do tipo {DocumentType} marcado para revisão manual.", type);

            return Task.FromResult(
                OcrResult.Failure("Serviço de OCR não configurado. Revisão manual necessária."));
        }
    }
}
