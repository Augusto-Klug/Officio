using ContaFacil.Application.Common;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;
using ContaFacil.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Features.Prospects.Commands.ConvertToClient;

public class ConvertProspectHandler(
    DbContext db,
    ICurrentTenant currentTenant) : IRequestHandler<ConvertProspectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        ConvertProspectCommand request,
        CancellationToken ct)
    {
        var prospect = await db.Set<Prospect>()
            .Include(p => p.Socios)
            .FirstOrDefaultAsync(p =>
                p.Id == request.ProspectId &&
                p.TenantId == currentTenant.TenantId, ct);

        if (prospect is null)
            return Result<Guid>.Failure("Prospect não encontrado.");

        if (prospect.Status == ProspectStatus.Convertido)
            return Result<Guid>.Failure("Prospect já foi convertido.");

        var client = Client.CreateFromProspect(prospect);
        db.Set<Client>().Add(client);

        foreach (var socio in prospect.Socios)
            client.AddSocio(Socio.CopyFor(socio, clientId: client.Id));

        prospect.MarkAsConverted(client.Id);

        await db.SaveChangesAsync(ct);

        return Result<Guid>.Success(client.Id);
    }
}
