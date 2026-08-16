using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Comum;
using Officio.Dominio.Interfaces;

namespace Officio.Infra.BancoDeDados.Repositorios;

public class RepositorioBase<TContexto, TChavePrimaria>(
    TContexto contexto) : IRepositorioBase<TContexto, TChavePrimaria>
    where TContexto : DbContext
{
    protected TContexto Contexto { get; } = contexto;

    public async Task<TEntidade?> ObterPorIdAsync<TEntidade>(
        TChavePrimaria id,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>().FindAsync([id], cancellationToken);

    public async Task<List<TEntidade>> ObterQuandoAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        bool rastreamento = true,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await AplicarRastreamento(Contexto.Set<TEntidade>(), rastreamento)
            .Where(exp)
            .ToListAsync(cancellationToken);

    public async Task<List<TEntidade>> ObterQuandoComExcluidosAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        bool rastreamento = true,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await AplicarRastreamento(Contexto.Set<TEntidade>().IgnoreQueryFilters(), rastreamento)
            .Where(exp)
            .ToListAsync(cancellationToken);

    public async Task<List<TEntidade>> ObterTodosAsync<TEntidade>(
        bool rastreamento = true,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await AplicarRastreamento(Contexto.Set<TEntidade>(), rastreamento)
            .ToListAsync(cancellationToken);

    public async Task<bool> VerificarExistenciaAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>().AnyAsync(exp, cancellationToken);

    public async Task<TEntidade?> PrimeiroOuPadraoAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>().FirstOrDefaultAsync(exp, cancellationToken);

    public async Task<TEntidade?> PrimeiroOuPadraoAsync<TEntidade, TKey>(
        Expression<Func<TEntidade, bool>> exp,
        Expression<Func<TEntidade, TKey>> ordem,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>()
            .Where(exp)
            .OrderBy(ordem)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TEntidade?> UltimoOuPadraoAsync<TEntidade, TKey>(
        Expression<Func<TEntidade, bool>> exp,
        Expression<Func<TEntidade, TKey>> ordem,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>()
            .Where(exp)
            .OrderByDescending(ordem)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<int> ContarAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>().CountAsync(exp, cancellationToken);

    public async Task AdicionarAsync<TEntidade>(
        TEntidade entidade,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>().AddAsync(entidade, cancellationToken);

    public async Task AdicionarAsync<TEntidade>(
        IList<TEntidade> entidades,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>().AddRangeAsync(entidades, cancellationToken);

    public Task AtualizarAsync<TEntidade>(
        TEntidade entidade,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
    {
        Contexto.Set<TEntidade>().Update(entidade);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync<TEntidade>(
        IList<TEntidade> entidades,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
    {
        Contexto.Set<TEntidade>().UpdateRange(entidades);
        return Task.CompletedTask;
    }

    public Task RemoverAsync<TEntidade>(
        TEntidade entidade,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
    {
        Contexto.Set<TEntidade>().Remove(entidade);
        return Task.CompletedTask;
    }

    public Task RemoverAsync<TEntidade>(
        IList<TEntidade> entidades,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
    {
        Contexto.Set<TEntidade>().RemoveRange(entidades);
        return Task.CompletedTask;
    }

    public IQueryable<TEntidade> MontarConsulta<TEntidade>(bool rastreamento = true)
        where TEntidade : EntidadeBase
        => AplicarRastreamento(Contexto.Set<TEntidade>(), rastreamento);

    public async Task<IEnumerable<TEntidade>> ExecutarSqlAsync<TEntidade>(
        FormattableString sql,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase
        => await Contexto.Set<TEntidade>()
            .FromSql(sql)
            .ToListAsync(cancellationToken);

    public async Task ExecutarComandoAsync(
        string sql,
        CancellationToken cancellationToken = default)
        => await Contexto.Database.ExecuteSqlRawAsync(sql, cancellationToken);

    public async Task ExecutarComandoAsync(
        string sql,
        IEnumerable<object> parametros,
        CancellationToken cancellationToken = default)
        => await Contexto.Database.ExecuteSqlRawAsync(sql, parametros, cancellationToken);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await Contexto.SaveChangesAsync(cancellationToken);

    private static IQueryable<TEntidade> AplicarRastreamento<TEntidade>(
        IQueryable<TEntidade> consulta,
        bool rastreamento)
        where TEntidade : EntidadeBase
        => rastreamento ? consulta : consulta.AsNoTracking();
}
