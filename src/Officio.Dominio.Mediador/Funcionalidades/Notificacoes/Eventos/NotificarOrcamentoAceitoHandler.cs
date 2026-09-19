using MediatR;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Eventos;

public class NotificarOrcamentoAceitoHandler(INotificacaoServico notificacaoServico)
    : INotificationHandler<EventoOrcamentoAceito>
{
    public Task Handle(EventoOrcamentoAceito notification, CancellationToken cancellationToken)
        => notificacaoServico.NotificarTenantAsync(
            notification.TenantId,
            "ORCAMENTO_ACEITO",
            "Orcamento aceito",
            "Um orcamento foi aceito e esta pronto para conversao em contrato.",
            notification.OrcamentoId,
            cancellationToken);
}
