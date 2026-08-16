using Officio.Dominio.Interfaces;

namespace Officio.WebApi.Intermediarios;

public class IntermediarioEmpresa(RequestDelegate next)
{
    private static readonly string[] RotasPublicas =
    [
        "/api/v1/autenticacao",
        "/api/v1/contratos/webhook/d4sign"
    ];

    public async Task InvokeAsync(HttpContext context, IEmpresaAtual currentTenant)
    {
        if (RotasPublicas.Any(rota => context.Request.Path.StartsWithSegments(rota)))
        {
            await next(context);
            return;
        }

        var tenantClaim = context.User.FindFirst("tenant_id");

        if (tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            currentTenant.SetTenant(tenantId);
        else if (context.Request.Path.StartsWithSegments("/api") &&
                 !RotasPublicas.Any(rota => context.Request.Path.StartsWithSegments(rota)))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Empresa nÃ£o identificado." });
            return;
        }

        await next(context);
    }
}





