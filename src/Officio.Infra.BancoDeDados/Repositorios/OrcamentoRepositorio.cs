using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class OrcamentoRepositorio(ContextoAplicacao contexto)
    : RepositorioBase<ContextoAplicacao, Guid>(contexto), IOrcamentoRepositorio
{
    public Task<Orcamento?> ObterPorIdComItensAsync(Guid orcamentoId, CancellationToken cancellationToken = default)
        => Contexto.Budgets
            .Include(orcamento => orcamento.Itens)
            .FirstOrDefaultAsync(orcamento => orcamento.Id == orcamentoId, cancellationToken);

    public Task<List<Orcamento>> ObterPorProspectoAsync(Guid prospectoId, CancellationToken cancellationToken = default)
        => Contexto.Budgets
            .Include(orcamento => orcamento.Itens)
            .Where(orcamento => orcamento.ProspectId == prospectoId)
            .OrderByDescending(orcamento => orcamento.CriadoEm)
            .ToListAsync(cancellationToken);

    public Task<ItemCatalogoOrcamento?> ObterItemCatalogoAsync(Guid itemId, CancellationToken cancellationToken = default)
        => Contexto.BudgetCatalogItems
            .FirstOrDefaultAsync(item => item.Id == itemId, cancellationToken);

    public Task<List<ItemCatalogoOrcamento>> ObterCatalogoAsync(bool incluirInativos, CancellationToken cancellationToken = default)
        => Contexto.BudgetCatalogItems
            .Where(item => incluirInativos || item.Ativo)
            .OrderBy(item => item.Secao)
            .ThenBy(item => item.Descricao)
            .ToListAsync(cancellationToken);

    public Task AdicionarAsync(Orcamento orcamento, CancellationToken cancellationToken = default)
        => AdicionarAsync<Orcamento>(orcamento, cancellationToken);

    public Task AdicionarItensAsync(IEnumerable<ItemOrcamento> itens, CancellationToken cancellationToken = default)
    {
        Contexto.BudgetItems.AddRange(itens);
        return Task.CompletedTask;
    }

    public Task AdicionarItemCatalogoAsync(ItemCatalogoOrcamento item, CancellationToken cancellationToken = default)
        => AdicionarAsync<ItemCatalogoOrcamento>(item, cancellationToken);

    public Task RemoverAsync(Orcamento orcamento, CancellationToken cancellationToken = default)
        => RemoverAsync<Orcamento>(orcamento, cancellationToken);
}
