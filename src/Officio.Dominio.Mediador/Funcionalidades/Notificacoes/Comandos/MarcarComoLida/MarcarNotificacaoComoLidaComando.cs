using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarComoLida;

public record MarcarNotificacaoComoLidaComando(Guid NotificacaoId)
    : IRequest<Resultado<bool>>;
