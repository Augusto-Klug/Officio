using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;
using Officio.WebApi.Extensoes;

namespace Officio.Testes.Unitarios.WebApi;

public class InicializacaoDesenvolvimentoTestes
{
    [Theory]
    [InlineData("Development", true, true, 1)]
    [InlineData("Development", false, true, 1)]
    [InlineData("Development", true, false, 0)]
    [InlineData("Production", true, true, 0)]
    public async Task PrepararBanco_UsaBootstrapOficialSemCriarEmpresa(
        string ambiente, bool prepararSchema, bool habilitarBootstrap, int usuariosEsperados)
    {
        var tenant = Guid.NewGuid();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = ambiente,
            ApplicationName = typeof(InicializacaoDesenvolvimentoTestes).Assembly.GetName().Name
        });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Database:EnsureCreatedOnStartup"] = prepararSchema.ToString(),
            ["BootstrapUser:Enabled"] = habilitarBootstrap.ToString(),
            ["BootstrapUser:TenantId"] = tenant.ToString(),
            ["BootstrapUser:TenantNome"] = "Parceiro de teste",
            ["BootstrapUser:TenantCnpj"] = "00999888000177",
            ["BootstrapUser:Nome"] = "Administrador",
            ["BootstrapUser:Email"] = "admin@test.local",
            ["BootstrapUser:Password"] = "senha-apenas-para-testes",
            ["BootstrapUser:Role"] = "Admin"
        });
        var banco = Guid.NewGuid().ToString();
        builder.Services.AddScoped(_ => new ContextoAplicacao(
            new DbContextOptionsBuilder<ContextoAplicacao>().UseInMemoryDatabase(banco).Options,
            new ServicoEmpresaAtual(), new PublicadorEventosFake()));
        await using var app = builder.Build();

        await app.PrepararBancoDesenvolvimentoAsync();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var usuarios = await db.Users.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(usuariosEsperados, usuarios.Count);
        Assert.Empty(await db.Empresas.ToListAsync());
        if (usuariosEsperados == 1)
            Assert.Equal(tenant, Assert.Single(usuarios).TenantId);
    }
}
