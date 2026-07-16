using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Identidade;
using Officio.Infraestrutura.Servicos;
using QuestPDF.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

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

        var ocrEndpoint = configuration["AzureOcr:Endpoint"];
        var ocrKey = configuration["AzureOcr:Key"];
        if (!string.IsNullOrEmpty(ocrEndpoint) && !string.IsNullOrEmpty(ocrKey))
        {
            services.AddSingleton(_ =>
                new DocumentAnalysisClient(new Uri(ocrEndpoint), new global::Azure.AzureKeyCredential(ocrKey)));
            services.AddScoped<IServicoOcr, ServicoOcrAzure>();
        }

        QuestPDF.Settings.License = LicenseType.Community;
        services.Configure<OpcoesPdf>(configuration.GetSection("Pdf"));
        services.AddScoped<IServicoPdf, ServicoPdfQuest>();

        services.Configure<OpcoesD4Sign>(configuration.GetSection("D4Sign"));
        services.AddHttpClient<IServicoAssinatura, ServicoD4Sign>(client =>
        {
            client.BaseAddress = new Uri(configuration["D4Sign:BaseUrl"] ?? "https://sandbox.d4sign.com.br/api/v1/");
        });

        services.AddScoped<IServicoEmail, ServicoEmail>();

        return services;
    }
}
