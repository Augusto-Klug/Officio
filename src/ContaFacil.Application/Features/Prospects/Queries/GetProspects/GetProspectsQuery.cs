using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Models;
using MediatR;

namespace ContaFacil.Application.Features.Prospects.Queries.GetProspects;

public record GetProspectsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Search = null,
    Guid? ResponsavelId = null) : IRequest<Result<PagedResult<ProspectDto>>>;
