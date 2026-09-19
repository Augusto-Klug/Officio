using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Consultas.ListarNotificacoes;

public record ListarNotificacoesConsulta(bool? Lida)
    : IRequest<Resultado<List<NotificacaoDto>>>;
