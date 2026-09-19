using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.ReenviarConvite;

public record ReenviarConviteComando(Guid ConviteId) : IRequest<Resultado<ConviteCriadoDto>>;
