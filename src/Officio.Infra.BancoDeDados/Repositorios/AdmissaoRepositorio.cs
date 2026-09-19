using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class AdmissaoRepositorio(ContextoAplicacao contexto)
    : RepositorioBase<ContextoAplicacao, Guid>(contexto), IAdmissaoRepositorio
{
    public Task<List<Admissao>> ListarAsync(
        Guid? empresaId,
        string? status,
        string? busca,
        CancellationToken cancellationToken = default)
    {
        var consulta = Contexto.Admissoes
            .AsNoTracking()
            .Include(admissao => admissao.Empresa)
            .Include(admissao => admissao.StatusAdmissao)
            .AsQueryable();

        if (empresaId.HasValue)
            consulta = consulta.Where(admissao => admissao.EmpresaId == empresaId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            consulta = consulta.Where(admissao => admissao.StatusAdmissao.Codigo == status.Trim().ToUpper());

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var documento = DocumentoFiscal.SomenteDigitos(termo);
            consulta = consulta.Where(admissao =>
                admissao.NomeFuncionario.Contains(termo) ||
                (admissao.Cpf != null &&
                    (admissao.Cpf.Contains(termo) ||
                     (documento != null && admissao.Cpf
                         .Replace(".", "")
                         .Replace("-", "")
                         .Contains(documento)))));
        }

        return consulta
            .OrderByDescending(admissao => admissao.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public Task<Admissao?> ObterDetalheAsync(Guid admissaoId, CancellationToken cancellationToken = default)
        => Contexto.Admissoes
            .AsNoTracking()
            .Include(admissao => admissao.Empresa)
            .Include(admissao => admissao.StatusAdmissao)
            .Include(admissao => admissao.Dependentes)
            .FirstOrDefaultAsync(admissao => admissao.Id == admissaoId, cancellationToken);

    public Task<Admissao?> ObterParaAlteracaoAsync(Guid admissaoId, CancellationToken cancellationToken = default)
        => Contexto.Admissoes
            .Include(admissao => admissao.Empresa)
            .Include(admissao => admissao.StatusAdmissao)
            .Include(admissao => admissao.Dependentes)
            .FirstOrDefaultAsync(admissao => admissao.Id == admissaoId, cancellationToken);

    public async Task<StatusAdmissao> ObterOuCriarStatusAsync(
        string codigo,
        string descricao,
        int ordem,
        CancellationToken cancellationToken = default)
    {
        var statusLocal = Contexto.StatusAdmissoes.Local.FirstOrDefault(item => item.Codigo == codigo);
        if (statusLocal is not null)
            return statusLocal;

        var status = await Contexto.StatusAdmissoes
            .FirstOrDefaultAsync(item => item.Codigo == codigo, cancellationToken);
        if (status is not null)
            return status;

        status = new StatusAdmissao(codigo, descricao, ordem);
        await Contexto.StatusAdmissoes.AddAsync(status, cancellationToken);
        return status;
    }

    public Task<EmpresaFuncionario?> ObterFuncionarioPorAdmissaoAsync(
        Guid admissaoId,
        CancellationToken cancellationToken = default)
        => Contexto.EmpresaFuncionarios
            .FirstOrDefaultAsync(funcionario => funcionario.AdmissaoOrigemId == admissaoId, cancellationToken);

    public Task AdicionarAsync(Admissao admissao, CancellationToken cancellationToken = default)
        => AdicionarAsync<Admissao>(admissao, cancellationToken);

    public Task AdicionarDependentesAsync(
        IEnumerable<AdmissaoDependente> dependentes,
        CancellationToken cancellationToken = default)
        => Contexto.AdmissaoDependentes.AddRangeAsync(dependentes, cancellationToken);

    public Task AdicionarFuncionarioAsync(EmpresaFuncionario funcionario, CancellationToken cancellationToken = default)
        => AdicionarAsync<EmpresaFuncionario>(funcionario, cancellationToken);
}
