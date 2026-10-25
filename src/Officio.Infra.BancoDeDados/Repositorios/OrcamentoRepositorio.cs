using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
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

    public Task<Orcamento?> ObterAceitoPorProspectoAsync(
        Guid prospectoId,
        Guid excetoOrcamentoId,
        CancellationToken cancellationToken = default)
        => Contexto.Budgets
            .Include(orcamento => orcamento.Itens)
            .FirstOrDefaultAsync(orcamento =>
                orcamento.ProspectId == prospectoId &&
                orcamento.Id != excetoOrcamentoId &&
                orcamento.Status == StatusOrcamento.Aceito,
                cancellationToken);

    public Task<bool> PossuiContratoAssociadoAsync(Guid orcamentoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts.AnyAsync(contrato => contrato.BudgetId == orcamentoId, cancellationToken);

    public async Task<T> ExecutarAceiteAtomicoAsync<T>(
        Func<CancellationToken, Task<T>> operacao,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var estrategia = Contexto.Database.CreateExecutionStrategy();
            return await estrategia.ExecuteAsync(async () =>
            {
                // Each retry must work with fresh entities; a failed attempt may have changed tracked state.
                Contexto.ChangeTracker.Clear();

                if (!Contexto.Database.IsRelational())
                    return await operacao(cancellationToken);

                await using var transacao = await Contexto.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
                var resultado = await operacao(cancellationToken);
                await transacao.CommitAsync(cancellationToken);
                return resultado;
            });
        }
        catch (DbUpdateException exception) when (EhViolacaoIndiceAceite(exception))
        {
            throw new InvalidOperationException(OrcamentoErros.AceiteConcorrente, exception);
        }
    }

    private static bool EhViolacaoIndiceAceite(DbUpdateException exception)
    {
        for (var atual = exception.InnerException; atual is not null; atual = atual.InnerException)
        {
            if (atual is SqlException sqlException
                && sqlException.Number is 2601 or 2627
                && sqlException.Message.Contains("IX_Budgets_TenantId_ProspectId_Aceito", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

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
