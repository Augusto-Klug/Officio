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
            .Include(admissao => admissao.Documentos)
                .ThenInclude(item => item.TipoDocumento)
            .Include(admissao => admissao.Documentos)
                .ThenInclude(item => item.Documento)
                    .ThenInclude(documento => documento!.Validacoes)
            .FirstOrDefaultAsync(admissao => admissao.Id == admissaoId, cancellationToken);

    public Task<Admissao?> ObterPorTokenHashAsync(
        string tokenHash,
        bool rastreamento,
        CancellationToken cancellationToken = default)
    {
        var consulta = Contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(admissao => admissao.Empresa)
            .Include(admissao => admissao.StatusAdmissao)
            .Include(admissao => admissao.Dependentes)
            .Include(admissao => admissao.Documentos)
                .ThenInclude(item => item.TipoDocumento)
            .Include(admissao => admissao.Documentos)
                .ThenInclude(item => item.Documento)
                    .ThenInclude(documento => documento!.Validacoes)
            .Where(admissao => admissao.TokenAcessoHash == tokenHash);

        if (!rastreamento)
            consulta = consulta.AsNoTracking();

        return consulta.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ConfigDocumentoAdmissao>> ObterOuCriarConfiguracoesDocumentoPadraoAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracoes = await Contexto.ConfigDocumentosAdmissao
            .Include(item => item.TipoDocumento)
            .Where(item => item.Ativo && item.TipoDocumento.Codigo != "CPF")
            .OrderBy(item => item.Ordem)
            .ToListAsync(cancellationToken);
        var padroes = new[]
        {
            new ConfiguracaoPadrao("IDENTIDADE", "Documento de identidade", "Identidade (RG/CIN)", false, 1),
            new ConfiguracaoPadrao("COMPROVANTE_RESIDENCIA", "Comprovante de residencia", "Comprovante de residência", true, 2),
            new ConfiguracaoPadrao("CARTEIRA_TRABALHO", "Carteira de trabalho", "Carteira de trabalho", true, 3),
            new ConfiguracaoPadrao("CNH", "Carteira Nacional de Habilitacao", "CNH", false, 4),
            new ConfiguracaoPadrao("TITULO_ELEITOR", "Titulo de eleitor", "Título de eleitor", false, 5),
            new ConfiguracaoPadrao("COMPROVANTE_ESCOLARIDADE", "Comprovante de escolaridade", "Comprovante de escolaridade", false, 6),
            new ConfiguracaoPadrao("CERTIFICADO_RESERVISTA", "Certificado de reservista", "Certificado de reservista", false, 7)
        };

        var codigos = padroes.Select(item => item.Codigo).ToList();
        var tiposExistentes = await Contexto.TiposDocumento
            .Where(item => codigos.Contains(item.Codigo))
            .ToDictionaryAsync(item => item.Codigo, cancellationToken);
        var codigosConfigurados = configuracoes
            .Select(item => item.TipoDocumento.Codigo)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var padrao in padroes)
        {
            if (codigosConfigurados.Contains(padrao.Codigo))
            {
                var existente = configuracoes.First(item => item.TipoDocumento.Codigo == padrao.Codigo);
                if (padrao.Codigo is "IDENTIDADE" or "CNH")
                    existente.AtualizarConfiguracao(padrao.Obrigatorio, padrao.Ordem, padrao.Rotulo);
                continue;
            }

            if (!tiposExistentes.TryGetValue(padrao.Codigo, out var tipo))
            {
                tipo = new TipoDocumentoDominio(padrao.Codigo, padrao.Descricao, padrao.Ordem);
                Contexto.TiposDocumento.Add(tipo);
                tiposExistentes[padrao.Codigo] = tipo;
            }

            configuracoes.Add(ConfigDocumentoAdmissao.Criar(
                tipo.Id, padrao.Obrigatorio, padrao.Ordem, padrao.Rotulo));
            codigosConfigurados.Add(padrao.Codigo);
        }

        var novasConfiguracoes = configuracoes
            .Where(item => Contexto.Entry(item).State == EntityState.Detached)
            .ToList();
        if (novasConfiguracoes.Count > 0)
            await Contexto.ConfigDocumentosAdmissao.AddRangeAsync(novasConfiguracoes, cancellationToken);

        return configuracoes.OrderBy(item => item.Ordem).ToList();
    }

    public Task<List<AdmissaoDocumento>> ListarDocumentosAsync(
        Guid admissaoId,
        CancellationToken cancellationToken = default)
        => Contexto.AdmissaoDocumentos
            .AsNoTracking()
            .Where(item => item.AdmissaoId == admissaoId && item.TipoDocumento.Codigo != "CPF")
            .Include(item => item.TipoDocumento)
            .Include(item => item.Documento)
                .ThenInclude(documento => documento!.Validacoes)
                    .ThenInclude(validacao => validacao.Divergencias)
            .OrderBy(item => item.Ordem)
            .ToListAsync(cancellationToken);

    public Task<AdmissaoDocumento?> ObterDocumentoAsync(
        Guid admissaoId,
        Guid admissaoDocumentoId,
        bool rastreamento,
        CancellationToken cancellationToken = default)
    {
        var consulta = Contexto.AdmissaoDocumentos
            .Where(item => item.AdmissaoId == admissaoId && item.Id == admissaoDocumentoId)
            .Include(item => item.Admissao)
                .ThenInclude(admissao => admissao.StatusAdmissao)
            .Include(item => item.TipoDocumento)
            .Include(item => item.Documento)
                .ThenInclude(documento => documento!.Validacoes)
                    .ThenInclude(validacao => validacao.Divergencias)
            .AsQueryable();

        if (!rastreamento)
            consulta = consulta.AsNoTracking();

        return consulta.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ValidacaoDocumento?> ObterUltimaValidacaoParaAlteracaoAsync(
        Guid documentoId,
        CancellationToken cancellationToken = default)
        => Contexto.DocumentValidations
            .Where(item => item.DocumentId == documentoId)
            .OrderByDescending(item => item.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

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

    public Task AdicionarDocumentoAsync(Documento documento, CancellationToken cancellationToken = default)
        => AdicionarAsync<Documento>(documento, cancellationToken);

    public Task RemoverDocumentoAsync(Documento documento, CancellationToken cancellationToken = default)
        => RemoverAsync(documento, cancellationToken);

    private sealed record ConfiguracaoPadrao(
        string Codigo,
        string Descricao,
        string Rotulo,
        bool Obrigatorio,
        int Ordem);
}
