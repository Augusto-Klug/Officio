using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarTodasComoLidas;

public class MarcarTodasNotificacoesComoLidasHandler(INotificacaoServico notificacaoServico)
    : IRequestHandler<MarcarTodasNotificacoesComoLidasComando, Resultado<int>>
{
    public Task<Resultado<int>> Handle(
        MarcarTodasNotificacoesComoLidasComando request,
        CancellationToken cancellationToken)
        => notificacaoServico.MarcarTodasComoLidasAsync(cancellationToken);
}
