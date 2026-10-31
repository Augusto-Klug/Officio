using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Interfaces;

public interface IContratoRepositorio
{
    Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken = default);
    Task<Guid?> ObterProspectoIdAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<List<Contrato>> ListarAsync(
        Guid? empresaId = null,
        StatusContrato? status = null,
        CancellationToken cancellationToken = default);
    Task<List<Contrato>> ListarAtivosPorEmpresaAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default);
    Task<List<Contrato>> ListarContratosExpiradosAsync(
        DateTime antesDe,
        CancellationToken cancellationToken = default);
    Task<List<Empresa>> ListarEmpresasComContratoAsync(CancellationToken cancellationToken = default);
    Task<HashSet<Guid>> ListarIdsComDocumentoAssinadoAsync(
        IReadOnlyCollection<Guid> contratoIds,
        CancellationToken cancellationToken = default);
    Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterParaGeracaoPdfAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterParaAssinaturaAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<AssinaturaSolicitacao?> ObterSolicitacaoAssinaturaPorIdExternoAsync(
        string provedor,
        string idExterno,
        CancellationToken cancellationToken = default);
    Task<List<SolicitacaoAssinaturaPendente>> ListarSolicitacoesAssinaturaParaReconciliacaoAsync(
        string provedor,
        CancellationToken cancellationToken = default);
    Task<StatusAssinatura> ObterOuCriarStatusAssinaturaAsync(
        string codigo,
        string descricao,
        int ordem,
        CancellationToken cancellationToken = default);
    Task<StatusCliente> ObterOuCriarStatusClienteAsync(
        string codigo,
        string descricao,
        int ordem,
        CancellationToken cancellationToken = default);
    Task AdicionarSolicitacaoAssinaturaAsync(
        AssinaturaSolicitacao solicitacao,
        CancellationToken cancellationToken = default);
    Task<bool> ExisteEventoAssinaturaAsync(
        Guid assinaturaSolicitacaoId,
        string tipoEvento,
        string? payload,
        CancellationToken cancellationToken = default);
    Task AdicionarEventoAssinaturaAsync(
        AssinaturaEvento evento,
        CancellationToken cancellationToken = default);
    Task<bool> ExisteDocumentoAssinadoAsync(
        Guid contratoId,
        CancellationToken cancellationToken = default);
    Task<Documento?> ObterDocumentoAssinadoAsync(
        Guid contratoId,
        CancellationToken cancellationToken = default);
    Task AdicionarDocumentoAsync(
        Documento documento,
        CancellationToken cancellationToken = default);
    Task AdicionarAssinaturaAsync(Assinatura assinatura, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}

public sealed record SolicitacaoAssinaturaPendente(
    Guid TenantId,
    string IdExterno,
    Guid ContratoId);
