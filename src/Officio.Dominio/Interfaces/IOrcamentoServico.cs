using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IOrcamentoServico
{
    Task<ResultadoOperacao<Guid>> CriarAsync(CriarOrcamentoRequisicao requisicao, CancellationToken ct = default);

    Task<ResultadoOperacao<Guid>> AtualizarAsync(AtualizarOrcamentoRequisicao requisicao, CancellationToken ct = default);

    Task<ResultadoOperacao<List<OrcamentoResultado>>> ObterPorProspectoAsync(
        Guid prospectoId,
        CancellationToken ct = default);

    Task<ResultadoOperacao<Guid>> CriarItemCatalogoAsync(
        CriarItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<Guid>> AtualizarItemCatalogoAsync(
        AtualizarItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<Guid>> AlterarAtivoItemCatalogoAsync(
        AlterarAtivoItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<List<CatalogoOrcamentoResultado>>> ListarCatalogoAsync(CancellationToken ct = default);

    Task<ResultadoOperacao<bool>> AlterarStatusAsync(
        AlterarStatusOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<bool>> AceitarAsync(Guid orcamentoId, CancellationToken ct = default);

    Task<ResultadoOperacao<bool>> RecusarAsync(RecusarOrcamentoRequisicao requisicao, CancellationToken ct = default);
}
