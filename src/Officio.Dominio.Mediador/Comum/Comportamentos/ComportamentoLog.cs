using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Officio.Dominio.Mediador.Comum.Comportamentos;

public class ComportamentoLog<TRequest, TResponse>(
    ILogger<ComportamentoLog<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var timer = Stopwatch.StartNew();
        logger.LogInformation("Iniciando request de aplicacao. RequestName={RequestName}", requestName);

        try
        {
            var response = await next(ct);
            logger.LogInformation(
                "Request de aplicacao finalizado. RequestName={RequestName} DuracaoMs={DuracaoMs}",
                requestName,
                timer.ElapsedMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Request de aplicacao lancou excecao. RequestName={RequestName} TipoExcecao={TipoExcecao} DuracaoMs={DuracaoMs}",
                requestName,
                exception.GetType().Name,
                timer.ElapsedMilliseconds);
            throw;
        }
    }
}





