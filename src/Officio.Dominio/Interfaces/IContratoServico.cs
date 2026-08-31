using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Interfaces;

public interface IContratoServico
{
    Task<Resultado<Guid>> ConverterOrcamentoAsync(
        ConverterOrcamentoContratoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<List<ContratoDto>>> ListarAsync(CancellationToken ct = default);

    Task<Resultado<ContratoDto>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default);

    Task<Resultado<ResultadoPdfContratoDto>> GerarPdfAsync(
        Guid contratoId,
        CancellationToken ct = default);

    Task<Resultado<SendForSignatureResultDto>> EnviarParaAssinaturaAsync(
        EnviarAssinaturaRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<bool>> ProcessarWebhookAutentiqueAsync(
        string payload,
        CancellationToken ct = default);
}
