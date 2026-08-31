using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;

public record ObterContratoPorIdConsulta(Guid ContratoId) : IRequest<Resultado<ContratoDto>>;
