using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;

namespace Officio.Testes.Unitarios.DominioServicos;

public class DashboardServicoTestes
{
    private static readonly DateTimeOffset Agora = new(2026, 9, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ListarAlertasAsync_UsaJanelasOperacionaisEOMaximoDeDozeItens()
    {
        var repositorio = new DashboardRepositorioFake();
        var servico = new DashboardServico(repositorio, new TimeProviderFixo(Agora));

        await servico.ListarAlertasAsync();

        Assert.Equal(new DateTime(2026, 10, 20, 23, 59, 59, 999).AddTicks(9999), repositorio.LimiteContratos);
        Assert.Equal(new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc), repositorio.InicioContratos);
        Assert.Equal(new DateTime(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc), repositorio.LimiteProspectos);
        Assert.Equal(12, repositorio.LimiteAlertas);
    }

    [Fact]
    public async Task ObterProjecaoReceitaAsync_CriaSeisMesesERespeitaVigencia()
    {
        var repositorio = new DashboardRepositorioFake
        {
            Contratos =
            [
                new(new DateTime(2026, 8, 1), null, 100m),
                new(new DateTime(2026, 10, 15), new DateTime(2026, 11, 10), 50m),
                new(new DateTime(2027, 1, 1), null, 25m)
            ]
        };
        var servico = new DashboardServico(repositorio, new TimeProviderFixo(Agora));

        var resultado = await servico.ObterProjecaoReceitaAsync();

        Assert.True(resultado.Sucesso);
        Assert.Equal(6, resultado.Valor!.Count);
        Assert.Equal(
            [100m, 150m, 150m, 100m, 125m, 125m],
            resultado.Valor.Select(item => item.ReceitaContratada));
        Assert.Equal(new DateOnly(2026, 9, 1), resultado.Valor[0].Mes);
        Assert.Equal(new DateOnly(2027, 2, 1), resultado.Valor[^1].Mes);
        Assert.Equal(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), repositorio.InicioProjecao);
    }

    private sealed class DashboardRepositorioFake : IDashboardRepositorio
    {
        public DateTime LimiteContratos { get; private set; }
        public DateTime InicioContratos { get; private set; }
        public DateTime LimiteProspectos { get; private set; }
        public int LimiteAlertas { get; private set; }
        public DateTime InicioProjecao { get; private set; }
        public List<DashboardContratoReceitaDto> Contratos { get; init; } = [];

        public Task<DashboardVisaoGeralDto> ObterVisaoGeralAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new DashboardVisaoGeralDto(0, 0, 0, 0, 0, 0));

        public Task<List<DashboardAlertaDto>> ListarAlertasAsync(
            DateTime inicioContratos,
            DateTime limiteContratos,
            DateTime limiteProspectosSemRetorno,
            int limite,
            CancellationToken cancellationToken = default)
        {
            InicioContratos = inicioContratos;
            LimiteContratos = limiteContratos;
            LimiteProspectos = limiteProspectosSemRetorno;
            LimiteAlertas = limite;
            return Task.FromResult(new List<DashboardAlertaDto>());
        }

        public Task<List<DashboardContratoReceitaDto>> ListarContratosParaProjecaoAsync(
            DateTime inicio,
            DateTime fim,
            CancellationToken cancellationToken = default)
        {
            InicioProjecao = inicio;
            return Task.FromResult(Contratos);
        }
    }

    private sealed class TimeProviderFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }
}
