using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IOrcamentoRepositorio
{
    Task<Orcamento?> ObterPorIdComItensAsync(Guid orcamentoId, CancellationToken cancellationToken = default);
    Task<Orcamento?> ObterAceitoPorProspectoAsync(Guid prospectoId, Guid excetoOrcamentoId, CancellationToken cancellationToken = default);
    Task<bool> PossuiContratoAssociadoAsync(Guid orcamentoId, CancellationToken cancellationToken = default);
    Task<T> ExecutarAceiteAtomicoAsync<T>(
        Func<CancellationToken, Task<T>> operacao,
        CancellationToken cancellationToken = default);
    Task<List<Orcamento>> ObterPorProspectoAsync(Guid prospectoId, CancellationToken cancellationToken = default);
    Task<ItemCatalogoOrcamento?> ObterItemCatalogoAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<List<ItemCatalogoOrcamento>> ObterCatalogoAsync(bool incluirInativos, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Orcamento orcamento, CancellationToken cancellationToken = default);
    Task AdicionarItensAsync(IEnumerable<ItemOrcamento> itens, CancellationToken cancellationToken = default);
    Task AdicionarItemCatalogoAsync(ItemCatalogoOrcamento item, CancellationToken cancellationToken = default);
    Task RemoverAsync(Orcamento orcamento, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
