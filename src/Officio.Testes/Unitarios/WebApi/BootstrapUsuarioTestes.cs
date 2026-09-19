using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;
using Officio.WebApi.Desenvolvimento;

namespace Officio.Testes.Unitarios.WebApi;

public class BootstrapUsuarioTestes
{
    [Theory]
    [InlineData("Production", true, true, 0)]
    [InlineData("Development", false, true, 0)]
    [InlineData("Development", true, false, 1)]
    [InlineData("Development", true, true, 1)]
    public async Task ExecutarAsync_RespeitaAmbienteEIdempotenciaSemDependerDeEmpresa(
        string ambiente, bool habilitado, bool empresaExiste, int quantidade)
    {
        var empresa = Empresa.Create("Empresa de teste", "");
        var tenantId = Guid.NewGuid();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BootstrapUser:Enabled"] = habilitado.ToString(),
            ["BootstrapUser:TenantId"] = tenantId.ToString(),
            ["BootstrapUser:Nome"] = "Maria",
            ["BootstrapUser:Email"] = "  maria@test.local  ",
            ["BootstrapUser:Password"] = "senha-apenas-para-testes",
            ["BootstrapUser:Role"] = "Admin"
        }).Build();
        var banco = Guid.NewGuid().ToString();
        var services = new ServiceCollection().AddLogging();
        services.AddScoped(_ => new ContextoAplicacao(
            new DbContextOptionsBuilder<ContextoAplicacao>().UseInMemoryDatabase(banco).Options,
            new ServicoEmpresaAtual(), new PublicadorEventosFake()));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        if (empresaExiste)
        {
            db.Empresas.Add(empresa);
            await db.SaveChangesAsync();
        }
        var environment = new AmbienteTeste { EnvironmentName = ambiente };
        await BootstrapUsuario.ExecutarAsync(provider, config, environment);
        await BootstrapUsuario.ExecutarAsync(provider, config, environment);
        var usuarios = await db.Users.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(quantidade, usuarios.Count);
        Assert.Equal(empresaExiste ? 1 : 0, await db.Empresas.CountAsync());
        if (quantidade == 1)
        {
            var usuario = Assert.Single(usuarios);
            Assert.Equal(tenantId, usuario.TenantId);
            Assert.Equal("maria@test.local", usuario.Email);
            Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<Usuario>()
                .VerifyHashedPassword(usuario, usuario.SenhaHash, "senha-apenas-para-testes"));
            config["Jwt:Key"] = "chave-exclusiva-para-testes-bootstrap-officio";
            var login = await new ServicoJwt(config, db).AutenticarAsync(usuario.Email, "senha-apenas-para-testes");
            Assert.NotNull(login);
            Assert.Equal("Admin", login.Usuario.Perfil);
            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(login.Token);
            Assert.Contains(jwt.Claims, c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
        }
    }

    private sealed class AmbienteTeste : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Officio.Testes";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
