using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Interfaces;

public interface IContratoServico
{
    Task<ResultadoOperacao<Guid>> ConverterOrcamentoAsync(
        ConverterOrcamentoContratoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<List<ContratoResultado>>> ListarAsync(CancellationToken ct = default);

    Task<ResultadoOperacao<ContratoResultado>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default);

    Task<ResultadoOperacao<PdfContratoResultado>> GerarPdfAsync(
        Guid contratoId,
        CancellationToken ct = default);

    Task<ResultadoOperacao<EnvioAssinaturaResultado>> EnviarParaAssinaturaAsync(
        EnviarAssinaturaRequisicao requisicao,
        CancellationToken ct = default);
}
