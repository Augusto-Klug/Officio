using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.ReenviarConviteAdministrador;

public record ReenviarConviteAdministradorComando(Guid EmpresaId) : IRequest<Resultado<ConviteCriadoDto>>;
