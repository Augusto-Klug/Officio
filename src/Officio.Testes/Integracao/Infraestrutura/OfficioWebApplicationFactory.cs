using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Seguranca;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Testes.Integracao.Infraestrutura;

public sealed class OfficioWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string banco = $"officio-integracao-{Guid.NewGuid()}";

    public ServicoEmailFake ServicoEmail { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Testing")
            .UseSetting("Jwt:Key", "officio-integration-tests-key-with-at-least-32-bytes")
            .UseSetting("Jwt:Issuer", "Officio.IntegrationTests")
            .UseSetting("Jwt:Audience", "Officio.IntegrationTests")
            .UseSetting("Database:EnsureCreatedOnStartup", "false");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "officio-integration-tests-key-with-at-least-32-bytes",
                ["Jwt:Issuer"] = "Officio.IntegrationTests",
                ["Jwt:Audience"] = "Officio.IntegrationTests",
                ["Database:EnsureCreatedOnStartup"] = "false",
                ["Email:ExporConteudoNoLog"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ContextoAplicacao>();
            services.RemoveAll<DbContext>();
            services.RemoveAll<DbContextOptions<ContextoAplicacao>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ContextoAplicacao>>();
            services.AddDbContext<ContextoAplicacao>(options => options.UseInMemoryDatabase(banco));
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<ContextoAplicacao>());

            services.RemoveAll<IServicoEmail>();
            services.AddSingleton<IServicoEmail>(ServicoEmail);
            services.RemoveAll<IServicoArmazenamento>();
            services.AddSingleton<IServicoArmazenamento, ServicoArmazenamentoFake>();
        });
    }

    public async Task ReiniciarBancoAsync()
    {
        ServicoEmail.Limpar();

        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        await contexto.Database.EnsureDeletedAsync();
        await contexto.Database.EnsureCreatedAsync();

        var usuario = Usuario.Create(
            Guid.NewGuid(),
            "Administrador da Plataforma",
            "plataforma@officio.test",
            string.Empty,
            PerfisAcesso.PlatformAdmin);
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "Senha@123"));
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();
    }

    public async Task<Guid> PrepararTenantAsync()
    {
        await ReiniciarBancoAsync();

        return await AdicionarTenantAsync(
            "99888777000166",
            "admin@integracao.test",
            "Tenant@123");
    }

    public async Task<Guid> AdicionarTenantAsync(string cnpj, string email, string senha)
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var parceiro = Parceiro.Criar(
            cnpj,
            $"Escritorio {cnpj}",
            $"Escritorio {cnpj}",
            email: $"contato-{cnpj}@escritorio.test");
        var usuario = Usuario.Create(
            parceiro.Id,
            "Administrador Integracao",
            email,
            string.Empty,
            PerfisAcesso.Admin);
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, senha));

        contexto.Parceiros.Add(parceiro);
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();

        return parceiro.Id;
    }
}
