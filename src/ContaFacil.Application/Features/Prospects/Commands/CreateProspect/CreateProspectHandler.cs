using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Features.Prospects.Commands.CreateProspect;

public class CreateProspectHandler(
    DbContext db,
    ICurrentTenant currentTenant) : IRequestHandler<CreateProspectCommand, Result<ProspectCreatedDto>>
{
    public async Task<Result<ProspectCreatedDto>> Handle(CreateProspectCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.Cnpj))
        {
            var exists = await db.Set<Prospect>()
                .AnyAsync(p => p.Cnpj == request.Cnpj, ct);
            if (exists)
                return Result<ProspectCreatedDto>.Failure("Já existe um prospect ou cliente com este CNPJ.");
        }

        var prospect = Prospect.Create(
            currentTenant.TenantId,
            request.RazaoSocial,
            request.Cnpj,
            request.RegimeTributario);

        db.Set<Prospect>().Add(prospect);

        if (request.Socios is { Count: > 0 })
        {
            foreach (var s in request.Socios)
            {
                var socio = Socio.Create(currentTenant.TenantId, s.Nome, s.Cpf, s.Participacao, prospectId: prospect.Id);
                db.Set<Socio>().Add(socio);
            }
        }

        await db.SaveChangesAsync(ct);

        return Result<ProspectCreatedDto>.Success(new ProspectCreatedDto(prospect.Id, prospect.RazaoSocial));
    }
}
