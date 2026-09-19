using MediatR;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Eventos;

public class NotificarDocumentoRevisaoManualHandler(INotificacaoServico notificacaoServico)
    : INotificationHandler<EventoDocumentoRevisaoManual>
{
    public Task Handle(EventoDocumentoRevisaoManual notification, CancellationToken cancellationToken)
        => notificacaoServico.NotificarTenantAsync(
            notification.TenantId,
            "DOCUMENTO_REVISAO_MANUAL",
            "Documento aguardando revisao",
            "Um documento precisa de revisao manual.",
            notification.DocumentId,
            cancellationToken);
}
