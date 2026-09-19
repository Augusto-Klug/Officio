using MediatR;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;

namespace Officio.WebApi.Servicos.Webhooks;

internal sealed class ProcessadorWebhookAutentique(
    IFilaWebhookAutentique fila,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ProcessadorWebhookAutentique> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var payload in fila.LerTodosAsync(stoppingToken))
            await ProcessarAsync(payload, stoppingToken);
    }

    internal async Task ProcessarAsync(string payload, CancellationToken cancellationToken)
    {
        try
        {
            await using var escopo = serviceScopeFactory.CreateAsyncScope();
            var mediator = escopo.ServiceProvider.GetRequiredService<IMediator>();
            var resultado = await mediator.Send(
                new ProcessarWebhookAutentiqueComando(payload),
                cancellationToken);

            if (!resultado.Sucesso)
            {
                logger.LogWarning(
                    "Falha ao processar webhook Autentique em segundo plano: {Erro}",
                    resultado.Erro);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar webhook Autentique em segundo plano.");
        }
    }
}
