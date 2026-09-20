using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IAdmissaoRepositorio
{
    Task<List<Admissao>> ListarAsync(
        Guid? empresaId,
        string? status,
        string? busca,
        CancellationToken cancellationToken = default);

    Task<Admissao?> ObterDetalheAsync(Guid admissaoId, CancellationToken cancellationToken = default);
    Task<Admissao?> ObterParaAlteracaoAsync(Guid admissaoId, CancellationToken cancellationToken = default);
    Task<List<ConfigDocumentoAdmissao>> ObterOuCriarConfiguracoesDocumentoPadraoAsync(CancellationToken cancellationToken = default);
    Task<List<AdmissaoDocumento>> ListarDocumentosAsync(Guid admissaoId, CancellationToken cancellationToken = default);
    Task<AdmissaoDocumento?> ObterDocumentoAsync(Guid admissaoId, Guid admissaoDocumentoId, bool rastreamento, CancellationToken cancellationToken = default);
    Task<ValidacaoDocumento?> ObterUltimaValidacaoParaAlteracaoAsync(Guid documentoId, CancellationToken cancellationToken = default);
    Task<StatusAdmissao> ObterOuCriarStatusAsync(string codigo, string descricao, int ordem, CancellationToken cancellationToken = default);
    Task<EmpresaFuncionario?> ObterFuncionarioPorAdmissaoAsync(Guid admissaoId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Admissao admissao, CancellationToken cancellationToken = default);
    Task AdicionarDependentesAsync(IEnumerable<AdmissaoDependente> dependentes, CancellationToken cancellationToken = default);
    Task AdicionarFuncionarioAsync(EmpresaFuncionario funcionario, CancellationToken cancellationToken = default);
    Task AdicionarDocumentoAsync(Documento documento, CancellationToken cancellationToken = default);
    Task RemoverDocumentoAsync(Documento documento, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
