using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Modulos;
using Officio.Infra.BancoDeDados.Modulos;
using Officio.Infraestrutura.Modulos;
using Officio.WebApi.Conversores;
using Officio.WebApi.Servicos.Webhooks;
using Officio.WebApi.TempoReal;
using System.Text;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesServiceCollection
{
    public static IServiceCollection AdicionarCorsFrontend(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origens = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
            .Where(origem => !string.IsNullOrWhiteSpace(origem))
            .Select(origem => origem.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (origens.Length > 0)
                policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod();
        }));

        return services;
    }

    public static IServiceCollection AdicionarServicosOfficio(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AdicionarMediadores(configuration);
        services.AdicionarBancoDeDados(configuration);
        services.AdicionarInfraestrutura(configuration);
        services.AddSignalR();
        services.AddSingleton<IPublicadorAtualizacaoTempoReal, PublicadorAtualizacaoSignalR>();

        services.AddHttpContextAccessor();
        services.AddScoped<IProspectoViewModelConversor, ProspectoViewModelConversor>();
        services.AddScoped<IDocumentoViewModelConversor, DocumentoViewModelConversor>();
        services.AddScoped<IContratoViewModelConversor, ContratoViewModelConversor>();
        services.AddScoped<IFilaWebhookAutentique, FilaWebhookAutentique>();
        services.AddHostedService<ProcessadorWebhookAutentique>();

        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("A configuracao 'Jwt:Key' e obrigatoria para autenticacao JWT.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "Officio",
                    ValidAudience = configuration["Jwt:Audience"] ?? "Officio",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/api/v1/tempo-real"))
                            context.Token = token;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
