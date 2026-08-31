using MediatR;
using Microsoft.Extensions.Logging;

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
        logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next(ct);

        logger.LogInformation("Handled {RequestName}", requestName);

        return response;
    }
}





