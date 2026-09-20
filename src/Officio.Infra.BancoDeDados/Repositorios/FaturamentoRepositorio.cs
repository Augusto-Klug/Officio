using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class FaturamentoRepositorio(ContextoAplicacao contexto) : IFaturamentoRepositorio
{
    public Task<List<Contrato>> ListarContratosAptosAsync(DateOnly competencia, CancellationToken ct = default)
    {
        var inicio = competencia.ToDateTime(TimeOnly.MinValue);
        var fim = competencia.AddMonths(1).ToDateTime(TimeOnly.MinValue);
        return contexto.Contracts
            .Include(item => item.Orcamento).ThenInclude(item => item!.Itens)
            .Include(item => item.Cliente).ThenInclude(item => item!.Empresa)
            .Include(item => item.Empresa)
            .Include(item => item.Prospecto).ThenInclude(item => item.Empresa)
            .Where(item => item.Status == StatusContrato.Ativo
                           && item.DataInicio < fim
                           && (item.DataFim == null || item.DataFim >= inicio))
            .ToListAsync(ct);
    }

    public async Task<HashSet<Guid>> ListarContratosJaCalculadosAsync(DateOnly competencia, CancellationToken ct = default)
        => (await contexto.FaturamentosMensais
            .Where(item => item.Competencia == competencia)
            .Select(item => item.ContratoId)
            .ToListAsync(ct)).ToHashSet();

    public Task<List<FaturamentoMensal>> ListarAsync(DateOnly? competencia, string? status, CancellationToken ct = default)
    {
        var consulta = contexto.FaturamentosMensais
            .AsNoTracking()
            .Include(item => item.StatusFaturamento)
            .Include(item => item.Contrato).ThenInclude(item => item.Cliente)
            .Include(item => item.Contrato).ThenInclude(item => item.Prospecto)
            .AsQueryable();
        if (competencia.HasValue) consulta = consulta.Where(item => item.Competencia == competencia);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var codigoStatus = status.Trim().ToUpperInvariant();
            consulta = consulta.Where(item => item.StatusFaturamento.Codigo == codigoStatus);
        }
        return consulta.OrderByDescending(item => item.Competencia).ThenBy(item => item.Contrato.Numero).ToListAsync(ct);
    }

    public Task<List<FaturamentoMensal>> ListarParaRecalculoAsync(
        DateOnly competencia,
        CancellationToken ct = default)
        => contexto.FaturamentosMensais
            .Include(item => item.StatusFaturamento)
            .Include(item => item.Contrato).ThenInclude(item => item.Orcamento).ThenInclude(item => item!.Itens)
            .Include(item => item.Contrato).ThenInclude(item => item.Cliente).ThenInclude(item => item!.Empresa)
            .Include(item => item.Contrato).ThenInclude(item => item.Empresa)
            .Include(item => item.Contrato).ThenInclude(item => item.Prospecto).ThenInclude(item => item.Empresa)
            .Where(item => item.Competencia == competencia)
            .ToListAsync(ct);

    public Task<FaturamentoMensal?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => contexto.FaturamentosMensais.Include(item => item.StatusFaturamento).FirstOrDefaultAsync(item => item.Id == id, ct);

    public async Task<StatusFaturamento> ObterOuCriarStatusAsync(string codigo, string descricao, CancellationToken ct = default)
    {
        var existente = await contexto.StatusFaturamentos.FirstOrDefaultAsync(item => item.Codigo == codigo, ct);
        if (existente is not null) return existente;
        var status = new StatusFaturamento(codigo, descricao);
        await contexto.StatusFaturamentos.AddAsync(status, ct);
        return status;
    }

    public Task AdicionarAsync(FaturamentoMensal faturamento, CancellationToken ct = default)
        => contexto.FaturamentosMensais.AddAsync(faturamento, ct).AsTask();

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) => contexto.SaveChangesAsync(ct);
}
