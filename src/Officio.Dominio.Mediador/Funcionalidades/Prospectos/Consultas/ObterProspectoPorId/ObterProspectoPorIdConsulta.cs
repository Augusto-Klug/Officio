using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectoPorId;

public record ObterProspectoPorIdConsulta(Guid Id) : IRequest<Resultado<ProspectoDetalheDto>>;
