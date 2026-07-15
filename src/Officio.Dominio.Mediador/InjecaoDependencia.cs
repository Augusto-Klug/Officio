using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Mediador.Comum.Comportamentos;
using Officio.Dominio.Servicos;
using Officio.Infraestrutura;

namespace Officio.Dominio.Mediador;

public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarMediadores(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = typeof(InjecaoDependencia).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoLog<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoValidacao<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoAuditoria<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.AdicionarServicosDominio();
        services.AdicionarInfraestrutura(configuration);

        return services;
    }
}




