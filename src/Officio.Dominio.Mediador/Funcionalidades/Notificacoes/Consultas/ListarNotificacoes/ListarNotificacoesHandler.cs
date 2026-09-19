using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Consultas.ListarNotificacoes;

public class ListarNotificacoesHandler(INotificacaoServico notificacaoServico)
    : IRequestHandler<ListarNotificacoesConsulta, Resultado<List<NotificacaoDto>>>
{
    public Task<Resultado<List<NotificacaoDto>>> Handle(
        ListarNotificacoesConsulta request,
        CancellationToken cancellationToken)
        => notificacaoServico.ListarAsync(request.Lida, cancellationToken);
}
