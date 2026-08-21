using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;

public record ObterContratoPorIdConsulta(Guid ContratoId) : IRequest<Resultado<ContratoDto>>;
