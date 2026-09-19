using MediatR;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Eventos;

public class NotificarDocumentoReprovadoHandler(INotificacaoServico notificacaoServico)
    : INotificationHandler<EventoDocumentoValidado>
{
    public Task Handle(EventoDocumentoValidado notification, CancellationToken cancellationToken)
        => notification.Valido
            ? Task.CompletedTask
            : notificacaoServico.NotificarTenantAsync(
                notification.TenantId,
                "DOCUMENTO_REPROVADO",
                "Documento reprovado",
                "Um documento foi reprovado durante a validacao.",
                notification.DocumentId,
                cancellationToken);
}
