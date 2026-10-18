using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Modulos;
using Officio.Infraestrutura.Integracoes.Autentique;
using Officio.Infraestrutura.Servicos;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class InfraestruturaInjecaoDependenciaTestes
{
    [Fact]
    public void AdicionarInfraestrutura_RegistraServicoAssinaturaEClienteAutentique()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Autentique:BaseUrl"] = "https://api.autentique.com.br/v2/graphql",
                ["Autentique:ApiToken"] = "",
                ["Autentique:WebhookSecret"] = "",
                ["Autentique:OrganizationId"] = "",
                ["Autentique:FolderId"] = ""
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AdicionarInfraestrutura(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.IsType<ServicoAutentique>(provider.GetRequiredService<IServicoAssinatura>());
        Assert.IsType<AutentiqueIntegracaoCliente>(provider.GetRequiredService<IAutentiqueIntegracaoCliente>());
    }

    [Fact]
    public void EnvExample_ContemVariaveisAutentique()
    {
        var conteudo = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, ".envexample"));

        Assert.Contains("Autentique__BaseUrl=https://api.autentique.com.br/v2/graphql", conteudo);
        Assert.Contains("Autentique__ApiToken=", conteudo);
        Assert.Contains("Autentique__WebhookSecret=", conteudo);
        Assert.Contains("Autentique__OrganizationId=", conteudo);
        Assert.Contains("Autentique__FolderId=", conteudo);
    }

}
