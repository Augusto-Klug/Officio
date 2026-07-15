using Officio.Dominio.Entidades;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Servicos.Interfaces;

namespace Officio.Dominio.Mediador.Comum.Comportamentos;

public interface IAuditableCommand
{
    string EntityName { get; }
    Guid EntityId { get; }
}

public class ComportamentoAuditoria<TRequest, TResponse>(
    DbContext db,
    IEmpresaAtual tenant,
    IUsuarioAtual user)
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
            db.Set<RegistroAuditoria>().Add(new RegistroAuditoria
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





