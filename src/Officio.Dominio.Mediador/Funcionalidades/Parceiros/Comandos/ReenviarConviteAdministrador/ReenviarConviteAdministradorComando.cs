using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.ReenviarConviteAdministrador;

public record ReenviarConviteAdministradorComando(Guid ParceiroId) : IRequest<Resultado<ConviteCriadoDto>>;
