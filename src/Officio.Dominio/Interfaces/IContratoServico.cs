using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Interfaces;

public interface IContratoServico
{
    Task<Resultado<RepresentantesContratoDto>> ObterRepresentantesAsync(Guid orcamentoId, CancellationToken ct = default);
    Task<Resultado<Guid>> ConverterOrcamentoAsync(
        ConverterOrcamentoContratoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<List<ContratoAtivoResumoDto>>> ListarAtivosParaOrcamentoAsync(
        Guid orcamentoId,
        CancellationToken ct = default);

    Task<Resultado<List<ContratoDto>>> ListarAsync(
        Guid? empresaId = null,
        StatusContrato? status = null,
        CancellationToken ct = default);

    Task<Resultado<int>> EncerrarContratosExpiradosAsync(
        DateOnly dataReferencia,
        CancellationToken ct = default);

    Task<Resultado<List<EmpresaContratoFiltroDto>>> ListarEmpresasAsync(
        CancellationToken ct = default);

    Task<Resultado<ContratoDto>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default);

    Task<Resultado<SignatariosContratoDto>> ObterSignatariosAsync(Guid contratoId, CancellationToken ct = default);

    Task<Resultado<ResultadoPdfContratoDto>> GerarPdfAsync(
        Guid contratoId,
        CancellationToken ct = default);

    Task<Resultado<ResultadoPdfAssinadoContratoDto>> ObterPdfAssinadoAsync(
        Guid contratoId,
        CancellationToken ct = default);

    Task<Resultado<SendForSignatureResultDto>> EnviarParaAssinaturaAsync(
        EnviarAssinaturaRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<bool>> ProcessarWebhookAutentiqueAsync(
        string payload,
        CancellationToken ct = default);

    Task<Resultado<ResumoReconciliacaoAssinaturas>> ReconciliarAssinaturasAutentiqueAsync(
        CancellationToken ct = default);
}

public sealed record ResumoReconciliacaoAssinaturas(
    int Consultadas,
    int Concluidas,
    int Pendentes,
    int Falhas);
