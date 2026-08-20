using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ContratoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IContratoRepositorio
{
    public Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default)
        => AdicionarAsync<Contrato>(contrato, cancellationToken);

    public Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .FirstOrDefaultAsync(contrato => contrato.BudgetId == orcamentoId, cancellationToken);

    public Task<List<Contrato>> ListarAsync(CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .Include(contrato => contrato.Prospecto)
            .Include(contrato => contrato.Cliente)
            .OrderByDescending(contrato => contrato.CriadoEm)
            .ToListAsync(cancellationToken);

    public Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .Include(contrato => contrato.Prospecto)
            .Include(contrato => contrato.Cliente)
            .Include(contrato => contrato.Orcamento)
                .ThenInclude(orcamento => orcamento!.Itens)
            .FirstOrDefaultAsync(contrato => contrato.Id == contratoId, cancellationToken);

    public async Task<Contrato?> ObterParaGeracaoPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Cliente)
                .ThenInclude(cl => cl.Socios)
            .Include(c => c.Orcamento)
                .ThenInclude(o => o!.Itens)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

    public async Task<Contrato?> ObterParaAssinaturaAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

    public async Task AdicionarAssinaturaAsync(Assinatura assinatura, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Assinatura>(assinatura, cancellationToken);
}
