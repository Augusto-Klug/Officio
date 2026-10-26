using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IOrcamentoServico
{
    Task<Resultado<Guid>> CriarAsync(CriarOrcamentoRequisicao requisicao, CancellationToken ct = default);

    Task<Resultado<Guid>> AtualizarAsync(AtualizarOrcamentoRequisicao requisicao, CancellationToken ct = default);

    Task<Resultado<OrcamentoDto>> ObterPorIdAsync(Guid orcamentoId, CancellationToken ct = default);

    Task<Resultado<bool>> ExcluirAsync(Guid orcamentoId, CancellationToken ct = default);

    Task<Resultado<List<OrcamentoDto>>> ObterPorProspectoAsync(
        Guid prospectoId,
        CancellationToken ct = default);

    Task<Resultado<Guid>> CriarItemCatalogoAsync(
        CriarItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<Guid>> AtualizarItemCatalogoAsync(
        AtualizarItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<Guid>> AlterarAtivoItemCatalogoAsync(
        AlterarAtivoItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<List<CatalogoOrcamentoDto>>> ListarCatalogoAsync(bool incluirInativos = false, CancellationToken ct = default);

    Task<Resultado<bool>> AlterarStatusAsync(
        AlterarStatusOrcamentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<bool>> AceitarAsync(Guid orcamentoId, bool confirmarSubstituicao = false, CancellationToken ct = default);

    Task<Resultado<bool>> RecusarAsync(RecusarOrcamentoRequisicao requisicao, CancellationToken ct = default);
}
