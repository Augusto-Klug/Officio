using Officio.Dominio.Interfaces;
using Officio.Dominio.Seguranca;

namespace Officio.WebApi.Intermediarios;

public class IntermediarioEmpresa(RequestDelegate next)
{
    private static readonly string[] RotasPublicas =
    [
        "/api/v1/autenticacao",
        "/api/v1/admissoes-publicas",
        "/api/v1/contratos/webhook/autentique",
        "/api/v1/interno/atualizacoes-tempo-real"
    ];

    public async Task InvokeAsync(
        HttpContext context,
        IEmpresaAtual currentTenant,
        IVerificadorAcesso verificadorAcesso)
    {
        if (RotasPublicas.Any(rota => context.Request.Path.StartsWithSegments(rota)))
        {
            await next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/v1/admin"))
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (!context.User.IsInRole(PerfisAcesso.PlatformAdmin))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (!ObterUsuarioId(context, out var usuarioId) ||
                !await verificadorAcesso.UsuarioAtivoAsync(usuarioId, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await next(context);
            return;
        }

        var tenantClaim = context.User.FindFirst("tenant_id");

        if (tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tenantId))
        {
            currentTenant.SetTenant(tenantId);
            if (!ObterUsuarioId(context, out var usuarioId) ||
                !await verificadorAcesso.UsuarioAtivoAsync(usuarioId, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (!await verificadorAcesso.ParceiroAtivoAsync(tenantId, context.RequestAborted))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Parceiro inativo." });
                return;
            }
        }
        else if (context.Request.Path.StartsWithSegments("/api") &&
                 !RotasPublicas.Any(rota => context.Request.Path.StartsWithSegments(rota)))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Parceiro não identificado." });
            return;
        }

        await next(context);
    }

    private static bool ObterUsuarioId(HttpContext context, out Guid usuarioId)
        => Guid.TryParse(
            context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            out usuarioId);
}





