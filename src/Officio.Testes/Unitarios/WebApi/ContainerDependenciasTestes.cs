using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infraestrutura.Servicos;
using Officio.WebApi.Extensoes;

namespace Officio.Testes.Unitarios.WebApi;

public class ContainerDependenciasTestes
{
    [Theory]
    [InlineData(null, null, false)]
    [InlineData("https://officio-testes.cognitiveservices.azure.com/", "chave-teste", true)]
    [InlineData("http://officio-testes.cognitiveservices.azure.com/", "chave-teste", false)]
    [InlineData("endpoint-invalido", "chave-teste", false)]
    [InlineData("https://officio-testes.cognitiveservices.azure.com/", "", false)]
    [InlineData("https://officio-testes.cognitiveservices.azure.com/", null, false)]
    [InlineData("https://officio-testes.cognitiveservices.azure.com/", "   ", false)]
    public void Container_ValidaGrafoCompletoEResolveImplementacoesExistentes(
        string? endpoint, string? chave, bool configurarOcr)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "chave-exclusiva-para-testes-container-officio",
            ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=Teste;Integrated Security=true",
            ["ConnectionStrings:AzureBlobStorage"] = "UseDevelopmentStorage=true",
            ["AzureOcr:Endpoint"] = endpoint,
            ["AzureOcr:Key"] = chave
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        services.AddSingleton<IConfiguration>(config);
        services.AdicionarServicosOfficio(config);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();
        Assert.IsType<ClienteServico>(scope.ServiceProvider.GetRequiredService<IClienteServico>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IEstimadorQualidadeDocumento>());
        if (configurarOcr)
        {
            Assert.IsType<ServicoOcrAzure>(scope.ServiceProvider.GetRequiredService<IServicoOcr>());
        }
        else
        {
            var tipoOcrIndisponivel = typeof(ServicoOcrAzure).Assembly.GetType(
                "Officio.Infraestrutura.Servicos.ServicoOcrIndisponivel", throwOnError: true)!;
            Assert.IsType(tipoOcrIndisponivel, scope.ServiceProvider.GetRequiredService<IServicoOcr>());
        }
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IDocumentoServico>());
    }
}
