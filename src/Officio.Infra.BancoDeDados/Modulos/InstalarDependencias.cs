using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;

namespace Microsoft.Extensions.DependencyInjection;

public static class InstalarDependenciasBancoDeDados
{
    public static IServiceCollection AdicionarBancoDeDados(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        servicos.AddDbContext<ContextoAplicacao>(options =>
            options.UseSqlServer(
                configuracao.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ContextoAplicacao).Assembly.FullName)));

        servicos.AddScoped<DbContext>(sp => sp.GetRequiredService<ContextoAplicacao>());
        servicos.AddScoped<IRepositorioBase<ContextoAplicacao, Guid>, RepositorioBase<ContextoAplicacao, Guid>>();
        servicos.AddScoped<IProspectoRepositorio, ProspectoRepositorio>();
        servicos.AddScoped<IDocumentoRepositorio, DocumentoRepositorio>();
        servicos.AddScoped<IContratoRepositorio, ContratoRepositorio>();
        servicos.AddScoped<IEmpresaRepositorio, EmpresaRepositorio>();

        return servicos;
    }
}
