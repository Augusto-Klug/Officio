using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarComoLida;

public class MarcarNotificacaoComoLidaHandler(INotificacaoServico notificacaoServico)
    : IRequestHandler<MarcarNotificacaoComoLidaComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(
        MarcarNotificacaoComoLidaComando request,
        CancellationToken cancellationToken)
        => notificacaoServico.MarcarComoLidaAsync(request.NotificacaoId, cancellationToken);
}
