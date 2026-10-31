using Officio.Dominio.Mediador.Comum.Excecoes;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Officio.WebApi.Intermediarios;

public class IntermediarioExcecao(RequestDelegate next, ILogger<IntermediarioExcecao> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ExcecaoValidacao ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { errors = ex.Errors });
        }
        catch (ExcecaoNaoEncontrado ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (ExcecaoNegocio ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Alteracao concorrente detectada. TraceId={TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new { error = "Os dados foram alterados por outra operacao. Atualize a tela e tente novamente.", traceId = context.TraceIdentifier });
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Erro nao tratado. TraceId={TraceId} Endpoint={Endpoint} TipoExcecao={TipoExcecao} StackTrace={StackTrace}",
                context.TraceIdentifier,
                context.GetEndpoint()?.DisplayName,
                ex.GetType().FullName,
                ex.StackTrace);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Erro interno do servidor.",
                traceId = context.TraceIdentifier
            });
        }
    }
}





