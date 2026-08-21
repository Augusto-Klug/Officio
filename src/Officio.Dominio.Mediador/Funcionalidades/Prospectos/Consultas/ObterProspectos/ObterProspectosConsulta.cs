using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;

public record ObterProspectosConsulta(ObterProspectosFiltro Filtro)
    : IRequest<Resultado<PagedResult<ProspectoDto>>>;

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
