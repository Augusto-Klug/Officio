using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Officio.Infra.BancoDeDados.Contextos;

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

        return servicos;
    }
}
