using MediatR;
using System.Diagnostics;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;

namespace Officio.WebApi.Servicos.Webhooks;

internal sealed class ProcessadorWebhookAutentique(
    IFilaWebhookAutentique fila,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ProcessadorWebhookAutentique> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in fila.LerTodosAsync(stoppingToken))
            await ProcessarAsync(item, stoppingToken);
    }

    internal async Task ProcessarAsync(WebhookAutentiqueEnfileirado item, CancellationToken cancellationToken)
    {
        using var atividade = MetricasWebhookAutentique.IniciarProcessamento(item.ContextoTrace);
        var inicio = Stopwatch.GetTimestamp();
        var sucesso = false;
        try
        {
            await using var escopo = serviceScopeFactory.CreateAsyncScope();
            var mediator = escopo.ServiceProvider.GetRequiredService<IMediator>();
            var resultado = await mediator.Send(
                new ProcessarWebhookAutentiqueComando(item.Payload),
                cancellationToken);

            if (!resultado.Sucesso)
            {
                logger.LogWarning(
                    "Falha ao processar webhook Autentique em segundo plano: {Erro}",
                    resultado.Erro);
            }
            sucesso = resultado.Sucesso;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao processar webhook Autentique em segundo plano.");
        }
        finally
        {
            var duracaoMs = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            MetricasWebhookAutentique.RegistrarProcessamento(sucesso, duracaoMs);
            atividade?.SetTag("webhook.processing.result", sucesso ? "success" : "failure");
            if (!sucesso)
                atividade?.SetStatus(ActivityStatusCode.Error);
        }
    }
}
