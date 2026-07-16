using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Officio.Infraestrutura;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
        => Microsoft.Extensions.DependencyInjection.InstalarDependenciasInfraestrutura
            .AdicionarInfraestrutura(services, configuration);
}
