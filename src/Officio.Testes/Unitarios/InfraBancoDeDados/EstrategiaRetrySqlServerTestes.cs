using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Modulos;
using Officio.Infraestrutura.Identidade;
using Officio.Testes.Dubles;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public sealed class EstrategiaRetrySqlServerTestes
{
    [Fact]
    public void AdicionarBancoDeDados_HabilitaRetryDoProviderSqlServer()
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=127.0.0.1,1434;Database=OfficioStaging;User Id=sa;Encrypt=False;TrustServerCertificate=True",
                ["Database:Password"] = "senha-de-teste"
            })
            .Build();

        var servicos = new ServiceCollection();
        servicos.AddLogging();
        servicos.AddSingleton<IEmpresaAtual, ServicoEmpresaAtual>();
        servicos.AddSingleton<IPublisher, PublicadorEventosFake>();
        servicos.AdicionarBancoDeDados(configuracao);

        using var provedor = servicos.BuildServiceProvider();
        using var escopo = provedor.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();

        Assert.Equal("SqlServerRetryingExecutionStrategy", contexto.Database.CreateExecutionStrategy().GetType().Name);
    }
}
