using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.CancelarConvite;

public record CancelarConviteComando(Guid ConviteId) : IRequest<Resultado<bool>>;
