using Microsoft.Extensions.Diagnostics.HealthChecks;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.WebApi.Saude;

public sealed class VerificacaoBancoDados(ContextoAplicacao contexto) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await contexto.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Banco de dados indisponivel.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Banco de dados indisponivel.", exception);
        }
    }
}
