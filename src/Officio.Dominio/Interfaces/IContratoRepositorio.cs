using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IContratoRepositorio
{
    Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken = default);
    Task<List<Contrato>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterParaGeracaoPdfAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterParaAssinaturaAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<AssinaturaSolicitacao?> ObterSolicitacaoAssinaturaPorIdExternoAsync(
        string provedor,
        string idExterno,
        CancellationToken cancellationToken = default);
    Task<StatusAssinatura> ObterOuCriarStatusAssinaturaAsync(
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
