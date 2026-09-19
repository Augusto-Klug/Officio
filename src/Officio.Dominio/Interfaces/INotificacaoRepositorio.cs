using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface INotificacaoRepositorio
{
    Task<List<Notificacao>> ListarAsync(
        Guid usuarioId,
        bool? lida = null,
        CancellationToken cancellationToken = default);

    Task<Notificacao?> ObterAsync(
        Guid notificacaoId,
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<List<Notificacao>> ListarNaoLidasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<List<Guid>> ListarUsuariosAtivosDoTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<List<Guid>> ListarUsuariosJaNotificadosAsync(
        Guid tenantId,
        string tipo,
        Guid referenciaId,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(
        IReadOnlyCollection<Notificacao> notificacoes,
        CancellationToken cancellationToken = default);

    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
