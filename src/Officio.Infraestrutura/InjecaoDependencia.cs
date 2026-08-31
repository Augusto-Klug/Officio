using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Infraestrutura.Modulos;

namespace Officio.Infraestrutura;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        IConfiguration configuration)
        => InstalarDependenciasInfraestrutura
            .AdicionarInfraestrutura(services, configuration);
}
