using Officio.Dominio.Mediador.Comum.Excecoes;
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro nÃ£o tratado");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Erro interno do servidor." });
        }
    }
}





