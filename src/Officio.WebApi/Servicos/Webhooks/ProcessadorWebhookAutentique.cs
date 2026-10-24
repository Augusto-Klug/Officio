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
                logger.LogError(
                    "Erro no ciclo da inbox Autentique. TipoExcecao={TipoExcecao} StackTrace={StackTrace}",
                    ex.GetType().FullName,
                    ex.StackTrace);
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
                    "Falha ao processar webhook Autentique. EventoId={EventoId} Tentativa={Tentativa} MotivoInformado={MotivoInformado}",
                    item.Id,
                    item.Tentativa,
                    resultado.Erro is not null);
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
            logger.LogError(
                "Erro ao processar webhook Autentique. EventoId={EventoId} Tentativa={Tentativa} TipoExcecao={TipoExcecao} StackTrace={StackTrace}",
                item.Id,
                item.Tentativa,
                ex.GetType().FullName,
                ex.StackTrace);
        }
        finally
        {
            var duracaoMs = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            MetricasWebhookAutentique.RegistrarProcessamento(sucesso, duracaoMs);
            logger.LogInformation(
                "Processamento de webhook Autentique finalizado. EventoId={EventoId} Tentativa={Tentativa} Resultado={Resultado} DuracaoMs={DuracaoMs}",
                item.Id,
                item.Tentativa,
                sucesso ? "sucesso" : "falha",
                duracaoMs);
            atividade?.SetTag("webhook.processing.result", sucesso ? "success" : "failure");
            if (!sucesso)
                atividade?.SetStatus(ActivityStatusCode.Error);
        }

        return (sucesso, erro);
    }
}
