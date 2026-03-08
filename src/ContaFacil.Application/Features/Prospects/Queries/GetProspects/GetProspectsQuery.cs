using ContaFacil.Application.Common;
using MediatR;

namespace ContaFacil.Application.Features.Prospects.Queries.GetProspects;

public record GetProspectsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Search = null,
    Guid? ResponsavelId = null) : IRequest<Result<PagedResult<ProspectDto>>>;

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
