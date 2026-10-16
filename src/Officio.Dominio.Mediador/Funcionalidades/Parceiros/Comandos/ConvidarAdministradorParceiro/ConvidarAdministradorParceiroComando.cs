using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.ConvidarAdministradorParceiro;

public record ConvidarAdministradorParceiroComando(Guid ParceiroId, string Nome, string Email)
    : IRequest<Resultado<ConviteCriadoDto>>;
