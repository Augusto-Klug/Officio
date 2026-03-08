using ContaFacil.Application.Common;
using ContaFacil.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Features.Prospects.Queries.GetProspects;

public class GetProspectsHandler(
    DbContext db) : IRequestHandler<GetProspectsQuery, Result<PagedResult<ProspectDto>>>
{
    public async Task<Result<PagedResult<ProspectDto>>> Handle(
        GetProspectsQuery request,
        CancellationToken ct)
    {
        var query = db.Set<Prospect>().AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(p =>
                p.RazaoSocial.Contains(request.Search) ||
                (p.Cnpj != null && p.Cnpj.Contains(request.Search)));

        if (request.ResponsavelId.HasValue)
            query = query.Where(p => p.ResponsavelId == request.ResponsavelId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderByDescending(p => p.CriadoEm)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProspectDto(
                p.Id,
                p.RazaoSocial,
                p.Cnpj,
                p.Status.ToString(),
                null,
                p.CriadoEm))
            .ToListAsync(ct);

        return Result<PagedResult<ProspectDto>>.Success(
            new PagedResult<ProspectDto>(data, totalCount, request.Page, request.PageSize, totalPages));
    }
}
