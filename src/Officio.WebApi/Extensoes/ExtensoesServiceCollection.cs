using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Officio.WebApi.Conversores;
using System.Text;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesServiceCollection
{
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
