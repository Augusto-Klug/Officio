using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Officio.Dominio.Mediador.Modulos;
using Officio.Infra.BancoDeDados.Modulos;
using Officio.Infraestrutura.Modulos;
using Officio.WebApi.Conversores;
using Officio.WebApi.Servicos.Webhooks;
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

        services.AddHttpContextAccessor();
        services.AddScoped<IProspectoViewModelConversor, ProspectoViewModelConversor>();
        services.AddScoped<IDocumentoViewModelConversor, DocumentoViewModelConversor>();
        services.AddScoped<IContratoViewModelConversor, ContratoViewModelConversor>();
        services.AddSingleton<IFilaWebhookAutentique, FilaWebhookAutentique>();
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
            });

        services.AddAuthorization();

        return services;
    }
}
