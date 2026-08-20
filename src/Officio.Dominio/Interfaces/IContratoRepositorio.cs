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
    Task AdicionarAssinaturaAsync(Assinatura assinatura, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
