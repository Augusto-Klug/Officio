using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface INotificacaoServico
{
    Task<Resultado<List<NotificacaoDto>>> ListarAsync(
        bool? lida = null,
        CancellationToken cancellationToken = default);

    Task<Resultado<bool>> MarcarComoLidaAsync(
        Guid notificacaoId,
        CancellationToken cancellationToken = default);

    Task<Resultado<int>> MarcarTodasComoLidasAsync(
        CancellationToken cancellationToken = default);

    Task NotificarTenantAsync(
        Guid tenantId,
        string tipo,
        string titulo,
        string? mensagem,
        Guid referenciaId,
        CancellationToken cancellationToken = default);
}
