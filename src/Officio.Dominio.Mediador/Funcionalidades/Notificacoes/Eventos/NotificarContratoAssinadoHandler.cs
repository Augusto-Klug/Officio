using MediatR;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Eventos;

public class NotificarContratoAssinadoHandler(INotificacaoServico notificacaoServico)
    : INotificationHandler<EventoContratoAssinado>
{
    public Task Handle(EventoContratoAssinado notification, CancellationToken cancellationToken)
        => notificacaoServico.NotificarTenantAsync(
            notification.TenantId,
            "CONTRATO_ASSINADO",
            "Contrato assinado",
            "O contrato foi assinado e esta ativo.",
            notification.ContractId,
            cancellationToken);
}
