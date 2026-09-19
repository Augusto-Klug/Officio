using MediatR;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Eventos;

public class NotificarProspectoConvertidoHandler(INotificacaoServico notificacaoServico)
    : INotificationHandler<EventoProspectoConvertido>
{
    public Task Handle(EventoProspectoConvertido notification, CancellationToken cancellationToken)
        => notificacaoServico.NotificarTenantAsync(
            notification.TenantId,
            "PROSPECTO_CONVERTIDO",
            "Prospecto convertido",
            "O prospecto foi convertido em cliente.",
            notification.ProspectId,
            cancellationToken);
}
