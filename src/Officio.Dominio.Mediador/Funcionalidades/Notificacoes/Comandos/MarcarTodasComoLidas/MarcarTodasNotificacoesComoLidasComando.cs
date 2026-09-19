using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarTodasComoLidas;

public record MarcarTodasNotificacoesComoLidasComando : IRequest<Resultado<int>>;
