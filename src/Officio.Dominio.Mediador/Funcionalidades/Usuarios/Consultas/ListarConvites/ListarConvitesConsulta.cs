using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Consultas.ListarConvites;

public record ListarConvitesConsulta : IRequest<Resultado<List<ConviteUsuarioDto>>>;
