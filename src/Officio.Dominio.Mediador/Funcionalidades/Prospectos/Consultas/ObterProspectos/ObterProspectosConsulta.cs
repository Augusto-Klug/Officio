using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;

public record ObterProspectosConsulta(ObterProspectosFiltro Filtro)
    : IRequest<Resultado<PagedResult<ProspectoDto>>>;
