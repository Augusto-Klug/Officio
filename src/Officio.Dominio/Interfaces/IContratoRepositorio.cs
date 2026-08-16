using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IContratoRepositorio
{
    Task<Contrato?> ObterParaGeracaoPdfAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task<Contrato?> ObterParaAssinaturaAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task AdicionarAssinaturaAsync(Assinatura assinatura, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
