using System.Net;
using System.Text.Json;
using ContaFacil.Application.Common.Exceptions;

namespace ContaFacil.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (BusinessException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Erro interno do servidor." });
        }
    }
}
