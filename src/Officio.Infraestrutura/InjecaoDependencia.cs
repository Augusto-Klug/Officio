using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using Officio.Dominio.Servicos.Interfaces;
using Officio.Infraestrutura.Identidade;
using Officio.Infraestrutura.Persistencia;
using Officio.Infraestrutura.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core
        services.AddDbContext<ContextoAplicacao>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ContextoAplicacao).Assembly.FullName)));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ContextoAplicacao>());

        // Identity
        services.AddScoped<IEmpresaAtual, ServicoEmpresaAtual>();
        services.AddScoped<IUsuarioAtual, ServicoUsuarioAtual>();
        services.AddScoped<ServicoJwt>();

        // Storage
        services.AddSingleton(_ =>
            new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")));
        services.AddScoped<IServicoArmazenamento, ServicoArmazenamentoAzureBlob>();

        // OCR
        var ocrEndpoint = configuration["AzureOcr:Endpoint"];
        var ocrKey = configuration["AzureOcr:Key"];
        if (!string.IsNullOrEmpty(ocrEndpoint) && !string.IsNullOrEmpty(ocrKey))
        {
            services.AddSingleton(_ =>
                new DocumentAnalysisClient(new Uri(ocrEndpoint), new Azure.AzureKeyCredential(ocrKey)));
            services.AddScoped<IServicoOcr, ServicoOcrAzure>();
        }

        // PDF
        QuestPDF.Settings.License = LicenseType.Community;
        services.Configure<OpcoesPdf>(configuration.GetSection("Pdf"));
        services.AddScoped<IServicoPdf, ServicoPdfQuest>();

        // D4Sign
        services.Configure<OpcoesD4Sign>(configuration.GetSection("D4Sign"));
        services.AddHttpClient<IServicoAssinatura, ServicoD4Sign>(client =>
        {
            client.BaseAddress = new Uri(configuration["D4Sign:BaseUrl"] ?? "https://sandbox.d4sign.com.br/api/v1/");
        });

        // Email
        services.AddScoped<IServicoEmail, ServicoEmail>();

        return services;
    }
}





