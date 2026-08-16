using System.Linq.Expressions;
using Officio.Dominio.Comum;

namespace Officio.Dominio.Interfaces;

public interface IRepositorioBase<TContexto, TChavePrimaria>
{
    Task<TEntidade?> ObterPorIdAsync<TEntidade>(
        TChavePrimaria id,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<List<TEntidade>> ObterQuandoAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        bool rastreamento = true,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<List<TEntidade>> ObterQuandoComExcluidosAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        bool rastreamento = true,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<List<TEntidade>> ObterTodosAsync<TEntidade>(
        bool rastreamento = true,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<bool> VerificarExistenciaAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<TEntidade?> PrimeiroOuPadraoAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<TEntidade?> PrimeiroOuPadraoAsync<TEntidade, TKey>(
        Expression<Func<TEntidade, bool>> exp,
        Expression<Func<TEntidade, TKey>> ordem,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<TEntidade?> UltimoOuPadraoAsync<TEntidade, TKey>(
        Expression<Func<TEntidade, bool>> exp,
        Expression<Func<TEntidade, TKey>> ordem,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task<int> ContarAsync<TEntidade>(
        Expression<Func<TEntidade, bool>> exp,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task AdicionarAsync<TEntidade>(
        TEntidade entidade,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task AdicionarAsync<TEntidade>(
        IList<TEntidade> entidades,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task AtualizarAsync<TEntidade>(
        TEntidade entidade,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task AtualizarAsync<TEntidade>(
        IList<TEntidade> entidades,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task RemoverAsync<TEntidade>(
        TEntidade entidade,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task RemoverAsync<TEntidade>(
        IList<TEntidade> entidades,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    IQueryable<TEntidade> MontarConsulta<TEntidade>(bool rastreamento = true)
        where TEntidade : EntidadeBase;

    Task<IEnumerable<TEntidade>> ExecutarSqlAsync<TEntidade>(
        FormattableString sql,
        CancellationToken cancellationToken = default)
        where TEntidade : EntidadeBase;

    Task ExecutarComandoAsync(
        string sql,
        CancellationToken cancellationToken = default);

    Task ExecutarComandoAsync(
        string sql,
        IEnumerable<object> parametros,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
