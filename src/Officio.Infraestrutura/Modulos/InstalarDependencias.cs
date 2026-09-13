using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddSingleton(_ =>
            new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")));
        services.AddScoped<IServicoArmazenamento, ServicoArmazenamentoAzureBlob>();

        services.AddSingleton(_ =>
        {
            var ocrEndpoint = configuration["AzureOcr:Endpoint"];
            var ocrKey = configuration["AzureOcr:Key"];
            if (!Uri.TryCreate(ocrEndpoint, UriKind.Absolute, out var endpoint) ||
                (endpoint.Scheme != Uri.UriSchemeHttps && endpoint.Scheme != Uri.UriSchemeHttp) ||
                string.IsNullOrWhiteSpace(ocrKey))
                throw new InvalidOperationException(
                    "Configure 'AzureOcr:Endpoint' com uma URL HTTP/HTTPS valida e 'AzureOcr:Key' para utilizar o servico de OCR.");

            return new DocumentAnalysisClient(endpoint, new global::Azure.AzureKeyCredential(ocrKey));
        });
        services.AddScoped<IServicoOcr, ServicoOcrAzure>();

        QuestPDF.Settings.License = LicenseType.Community;
        services.Configure<OpcoesPdf>(configuration.GetSection("Pdf"));
        services.AddScoped<IServicoPdf, ServicoPdfQuest>();

        services.Configure<OpcoesAutentique>(configuration.GetSection("Autentique"));
        services.AddHttpClient<IClienteIntegracao<OpcoesAutentique>, ClienteIntegracao<OpcoesAutentique>>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Autentique:BaseUrl"] ?? "https://api.autentique.com.br/v2/graphql");
        });
        services.AddScoped<IAutentiqueIntegracaoCliente, AutentiqueIntegracaoCliente>();
        services.AddScoped<IServicoAssinatura, ServicoAutentique>();

        services.AddScoped<IServicoEmail, ServicoEmail>();

        return services;
    }
}
