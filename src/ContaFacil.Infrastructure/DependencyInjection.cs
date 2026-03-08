using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Infrastructure.Identity;
using ContaFacil.Infrastructure.Persistence;
using ContaFacil.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace ContaFacil.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Identity
        services.AddScoped<ICurrentTenant, CurrentTenantService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<JwtService>();

        // Storage
        services.AddSingleton(_ =>
            new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")));
        services.AddScoped<IStorageService, AzureBlobStorageService>();

        // OCR
        var ocrEndpoint = configuration["AzureOcr:Endpoint"];
        var ocrKey = configuration["AzureOcr:Key"];
        if (!string.IsNullOrEmpty(ocrEndpoint) && !string.IsNullOrEmpty(ocrKey))
        {
            services.AddSingleton(_ =>
                new DocumentAnalysisClient(new Uri(ocrEndpoint), new Azure.AzureKeyCredential(ocrKey)));
            services.AddScoped<IOcrService, AzureOcrService>();
        }

        // PDF
        QuestPDF.Settings.License = LicenseType.Community;
        services.Configure<PdfOptions>(configuration.GetSection("Pdf"));
        services.AddScoped<IPdfService, QuestPdfService>();

        // D4Sign
        services.Configure<D4SignOptions>(configuration.GetSection("D4Sign"));
        services.AddHttpClient<ISignatureService, D4SignService>(client =>
        {
            client.BaseAddress = new Uri(configuration["D4Sign:BaseUrl"] ?? "https://sandbox.d4sign.com.br/api/v1/");
        });

        // Email
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
