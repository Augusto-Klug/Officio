using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Officio.Dominio.Mediador;

public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarMediadores(
        this IServiceCollection services,
        IConfiguration configuration)
        => Microsoft.Extensions.DependencyInjection.InstalarDependenciasMediador
            .AdicionarMediadores(services, configuration);
}
