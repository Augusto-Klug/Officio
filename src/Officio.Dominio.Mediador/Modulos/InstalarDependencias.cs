using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum.Auditoria;
using Officio.Dominio.Mediador.Comum.Comportamentos;
using Officio.Dominio.Servicos.Modulos;

namespace Officio.Dominio.Mediador.Modulos;

public static class InstalarDependenciasMediador
{
    public static IServiceCollection AdicionarMediadores(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var assembly = typeof(InstalarDependenciasMediador).Assembly;

        servicos.AddScoped<IContextoAuditoria, ContextoAuditoria>();

        servicos.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoLog<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoValidacao<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ComportamentoAuditoria<,>));
        });

        servicos.AddValidatorsFromAssembly(assembly);
        servicos.AdicionarServicos(configuracao);

        return servicos;
    }
}
