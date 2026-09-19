using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Consultas.ListarUsuarios;

public record ListarUsuariosConsulta : IRequest<Resultado<List<UsuarioGestaoDto>>>;
