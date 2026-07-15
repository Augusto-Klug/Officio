using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;

public record ObterProspectosConsulta(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Search = null,
    Guid? ResponsavelId = null) : IRequest<Resultado<PagedResult<ProspectoDto>>>;

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);





