using Microsoft.EntityFrameworkCore;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class BloqueioFluxoContratosSqlTestes
{
    [SqlLocalFact]
    public async Task BloqueioDoMesmoProspectoAguardaLiberacaoDaPrimeiraSessao()
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlServer(Environment.GetEnvironmentVariable("OFFICIO_SQL_TEST_CONNECTION")).Options;
        var empresa = new ServicoEmpresaAtual();
        empresa.SetTenant(Guid.NewGuid());
        await using var primeiroContexto = new ContextoAplicacao(options, empresa, new PublicadorEventosFake());
        await using var segundoContexto = new ContextoAplicacao(options, empresa, new PublicadorEventosFake());
        var primeiro = new ProspectoRepositorio(primeiroContexto);
        var segundo = new ProspectoRepositorio(segundoContexto);
        var prospectoId = Guid.NewGuid();
        var bloqueio = await primeiro.BloquearFluxoContratosAsync(prospectoId);
        try
        {
            using var limite = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var espera = segundo.BloquearFluxoContratosAsync(prospectoId, limite.Token);
            await Task.Delay(300);
            Assert.False(espera.IsCompleted);
            await bloqueio.DisposeAsync();
            bloqueio = null!;
            await using var segundoBloqueio = await espera;
        }
        finally
        {
            if (bloqueio is not null) await bloqueio.DisposeAsync();
        }
    }
}
