using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Mediador.Comum.Comportamentos;

namespace Microsoft.Extensions.DependencyInjection;

public static class InstalarDependenciasMediador
{
    public static IServiceCollection AdicionarMediadores(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var assembly = typeof(InstalarDependenciasMediador).Assembly;

        servicos.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoLog<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoValidacao<,>));
        });

        servicos.AddValidatorsFromAssembly(assembly);
        servicos.AdicionarServicos(configuracao);

        return servicos;
    }
}
