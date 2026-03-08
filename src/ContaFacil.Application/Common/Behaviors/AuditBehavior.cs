using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContaFacil.Application.Common.Behaviors;

public interface IAuditableCommand
{
    string EntityName { get; }
    Guid EntityId { get; }
}

public class AuditBehavior<TRequest, TResponse>(
    DbContext db,
    ICurrentTenant tenant,
    ICurrentUser user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next(ct);

        if (request is IAuditableCommand auditable)
        {
            db.Set<AuditLog>().Add(new AuditLog
            {
                TenantId = tenant.TenantId,
                UserId = user.UserId,
                Entidade = auditable.EntityName,
                EntidadeId = auditable.EntityId,
                Acao = typeof(TRequest).Name,
                CriadoEm = DateTime.UtcNow,
            });

            await db.SaveChangesAsync(ct);
        }

        return response;
    }
}
