using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;

namespace Officio.Infra.BancoDeDados.Modulos;

public static class InstalarDependenciasBancoDeDados
{
    public static IServiceCollection AdicionarBancoDeDados(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        servicos.AddDbContext<ContextoAplicacao>(options =>
            options.UseSqlServer(
                ObterConnectionString(configuracao),
                b => b.MigrationsAssembly(typeof(ContextoAplicacao).Assembly.FullName)));

        servicos.AddScoped<DbContext>(sp => sp.GetRequiredService<ContextoAplicacao>());
        servicos.AddScoped<IRepositorioBase<ContextoAplicacao, Guid>, RepositorioBase<ContextoAplicacao, Guid>>();
        servicos.AddScoped<IProspectoRepositorio, ProspectoRepositorio>();
        servicos.AddScoped<IDocumentoRepositorio, DocumentoRepositorio>();
        servicos.AddScoped<IContratoRepositorio, ContratoRepositorio>();
        servicos.AddScoped<IEmpresaRepositorio, EmpresaRepositorio>();
        servicos.AddScoped<IOrcamentoRepositorio, OrcamentoRepositorio>();
        servicos.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        servicos.AddScoped<INotificacaoRepositorio, NotificacaoRepositorio>();
        servicos.AddScoped<IGestaoAcessoRepositorio, GestaoAcessoRepositorio>();
        servicos.AddScoped<IAdmissaoRepositorio, AdmissaoRepositorio>();
        servicos.AddScoped<IDashboardRepositorio, DashboardRepositorio>();
        servicos.AddScoped<IFaturamentoRepositorio, FaturamentoRepositorio>();
        servicos.AddScoped<IFormularioAberturaRepositorio, FormularioAberturaRepositorio>();
        servicos.AddScoped<IVerificadorAcesso, VerificadorAcesso>();

        return servicos;
    }

    private static string ObterConnectionString(IConfiguration configuracao)
    {
        var connectionString = configuracao.GetConnectionString("DefaultConnection");
        var senha = configuracao["Database:Password"];

        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(senha))
            return connectionString ?? string.Empty;

        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.Password))
            builder.Password = senha;

        return builder.ConnectionString;
    }
}
