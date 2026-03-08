using ContaFacil.Application;
using ContaFacil.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ContaFacil.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContaFacilServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

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
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "ContaFacil",
                    ValidAudience = configuration["Jwt:Audience"] ?? "ContaFacil",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "super-secret-key-contafacil-2025!"))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
