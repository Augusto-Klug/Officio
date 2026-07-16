using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "super-secret-key-officio-2025!"))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
