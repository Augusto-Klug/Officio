using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class DashboardServico(IDashboardRepositorio repositorio, TimeProvider timeProvider) : IDashboardServico
{
    public async Task<Resultado<DashboardVisaoGeralDto>> ObterVisaoGeralAsync(CancellationToken cancellationToken = default)
        => Resultado<DashboardVisaoGeralDto>.CriarSucesso(
            await repositorio.ObterVisaoGeralAsync(cancellationToken));

    public async Task<Resultado<List<DashboardAlertaDto>>> ListarAlertasAsync(CancellationToken cancellationToken = default)
    {
        var agora = timeProvider.GetUtcNow().UtcDateTime;
        var alertas = await repositorio.ListarAlertasAsync(
            agora.Date,
            agora.Date.AddDays(30).AddDays(1).AddTicks(-1),
            agora.AddDays(-7),
            12,
            cancellationToken);

        return Resultado<List<DashboardAlertaDto>>.CriarSucesso(alertas);
    }

    public async Task<Resultado<List<DashboardProjecaoReceitaDto>>> ObterProjecaoReceitaAsync(
        CancellationToken cancellationToken = default)
    {
        var hoje = timeProvider.GetUtcNow().UtcDateTime;
        var primeiroMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimPeriodo = primeiroMes.AddMonths(6).AddTicks(-1);
        var contratos = await repositorio.ListarContratosParaProjecaoAsync(
            primeiroMes,
            fimPeriodo,
            cancellationToken);

        var projecao = Enumerable.Range(0, 6)
            .Select(indice => primeiroMes.AddMonths(indice))
            .Select(mes => new DashboardProjecaoReceitaDto(
                DateOnly.FromDateTime(mes),
                contratos
                    .Where(item => item.DataInicio < mes.AddMonths(1)
                                   && (item.DataFim == null || item.DataFim >= mes))
                    .Sum(item => item.ReceitaMensal)))
            .ToList();

        return Resultado<List<DashboardProjecaoReceitaDto>>.CriarSucesso(projecao);
    }
}
