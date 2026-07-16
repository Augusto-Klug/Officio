using Officio.Dominio.Interfaces;

namespace Officio.WebApi.Intermediarios;

public class IntermediarioEmpresa(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IEmpresaAtual currentTenant)
    {
        var tenantClaim = context.User.FindFirst("tenant_id");

        if (tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            currentTenant.SetTenant(tenantId);
        else if (context.Request.Path.StartsWithSegments("/api") &&
                 !context.Request.Path.StartsWithSegments("/api/auth"))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Empresa nÃ£o identificado." });
            return;
        }

        await next(context);
    }
}





