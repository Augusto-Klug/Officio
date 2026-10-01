using MediatR;
using System.Diagnostics;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;

namespace Officio.WebApi.Servicos.Webhooks;

internal sealed class ProcessadorWebhookAutentique(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ProcessadorWebhookAutentique> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var escopoFila = serviceScopeFactory.CreateAsyncScope();
                var fila = escopoFila.ServiceProvider.GetRequiredService<IFilaWebhookAutentique>();
                var item = await fila.TentarReservarProximoAsync(stoppingToken);
                if (item is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                var resultado = await ProcessarAsync(item, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                    break;

                if (resultado.Sucesso)
                    await fila.MarcarProcessadoAsync(item.Id, stoppingToken);
                else
                    await fila.RegistrarFalhaAsync(item.Id, resultado.Erro!, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no ciclo de processamento da inbox de webhooks Autentique.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    internal async Task<(bool Sucesso, string? Erro)> ProcessarAsync(
        WebhookAutentiqueEnfileirado item,
        CancellationToken cancellationToken)
    {
        using var atividade = MetricasWebhookAutentique.IniciarProcessamento(item.ContextoTrace);
        var inicio = Stopwatch.GetTimestamp();
        var sucesso = false;
        string? erro = null;
        try
        {
            await using var escopo = serviceScopeFactory.CreateAsyncScope();
            var mediator = escopo.ServiceProvider.GetRequiredService<IMediator>();
            var resultado = await mediator.Send(
                new ProcessarWebhookAutentiqueComando(item.Payload),
                cancellationToken);

            if (!resultado.Sucesso)
            {
                erro = resultado.Erro ?? "O servico de dominio rejeitou o evento sem informar o motivo.";
                logger.LogWarning(
                    "Falha ao processar webhook Autentique. EventoId={EventoId} Tentativa={Tentativa} Erro={Erro}",
                    item.Id,
                    item.Tentativa,
                    erro);
            }
            sucesso = resultado.Sucesso;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            erro = $"{ex.GetType().Name}: {ex.Message}";
            logger.LogError(ex,
                "Erro ao processar webhook Autentique. EventoId={EventoId} Tentativa={Tentativa}",
                item.Id,
                item.Tentativa);
        }
        finally
        {
            var duracaoMs = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            MetricasWebhookAutentique.RegistrarProcessamento(sucesso, duracaoMs);
            atividade?.SetTag("webhook.processing.result", sucesso ? "success" : "failure");
            if (!sucesso)
                atividade?.SetStatus(ActivityStatusCode.Error);
        }

        return (sucesso, erro);
    }
}
