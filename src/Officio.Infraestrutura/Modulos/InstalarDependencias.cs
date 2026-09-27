using Azure.AI.DocumentIntelligence;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Integracoes;
using Officio.Infraestrutura.Integracoes.Autentique;
using Officio.Infraestrutura.Identidade;
using Officio.Infraestrutura.Servicos;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Modulos;

public static class InstalarDependenciasInfraestrutura
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IEmpresaAtual, ServicoEmpresaAtual>();
        services.AddScoped<IUsuarioAtual, ServicoUsuarioAtual>();
        services.AddScoped<IAutenticacaoServico, ServicoJwt>();
        services.AddScoped<IServicoSenha, ServicoSenha>();

        services.AddSingleton(_ =>
            new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")));
        services.AddScoped<IServicoArmazenamento, ServicoArmazenamentoAzureBlob>();

        var ocrEndpoint = configuration["AzureOcr:Endpoint"];
        var ocrKey = configuration["AzureOcr:Key"];

        if (Uri.TryCreate(ocrEndpoint, UriKind.Absolute, out var endpoint) &&
            endpoint.Scheme == Uri.UriSchemeHttps &&
            !string.IsNullOrWhiteSpace(ocrKey))
        {
            services.AddSingleton(_ =>
                new DocumentIntelligenceClient(
                    endpoint,
                    new global::Azure.AzureKeyCredential(ocrKey)));

            services.AddScoped<IServicoOcr, ServicoOcrAzure>();
        }
        else
        {
            services.AddScoped<IServicoOcr, ServicoOcrIndisponivel>();
        }

        QuestPDF.Settings.License = LicenseType.Community;
        services.Configure<OpcoesPdf>(configuration.GetSection("Pdf"));
        services.AddScoped<IServicoPdf, ServicoPdfQuest>();
        services.AddScoped<IConversorImagensPdf, ConversorImagensPdfQuest>();

        services.Configure<OpcoesAutentique>(configuration.GetSection("Autentique"));
        services.AddHttpClient<IClienteIntegracao<OpcoesAutentique>, ClienteIntegracao<OpcoesAutentique>>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Autentique:BaseUrl"] ?? "https://api.autentique.com.br/v2/graphql");
        });
        services.AddScoped<IAutentiqueIntegracaoCliente, AutentiqueIntegracaoCliente>();
        services.AddScoped<IServicoAssinatura, ServicoAutentique>();

        services.AddScoped<IServicoEmail, ServicoEmail>();

        services.RemoveAll<IConsultaMunicipioIbge>();
        services.AddHttpClient<IConsultaMunicipioIbge, ConsultaMunicipioIbge>(client =>
        {
            client.BaseAddress = new Uri("https://servicodados.ibge.gov.br/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.RemoveAll<IConsultaCep>();
        services.AddHttpClient<IConsultaCep, ConsultaCepViaCep>(client =>
        {
            client.BaseAddress = new Uri("https://viacep.com.br/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
