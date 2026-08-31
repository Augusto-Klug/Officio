using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Mediador.Modulos;

namespace Officio.Dominio.Mediador;

public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarMediadores(
        this IServiceCollection services,
        IConfiguration configuration)
        => InstalarDependenciasMediador
            .AdicionarMediadores(services, configuration);
}
