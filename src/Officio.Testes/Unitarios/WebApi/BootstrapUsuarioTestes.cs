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
    public async Task ExecutarAsync_RespeitaAmbienteECriaParceiroDeFormaIdempotente(
        string ambiente, bool habilitado, bool parceiroExiste, int quantidade)
    {
        var tenantId = Guid.NewGuid();
        var parceiro = Parceiro.CriarDesenvolvimento(tenantId, "00999888000177", "Parceiro de teste");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BootstrapUser:Enabled"] = habilitado.ToString(),
            ["BootstrapUser:TenantId"] = tenantId.ToString(),
            ["BootstrapUser:TenantNome"] = "Officio Desenvolvimento",
            ["BootstrapUser:TenantCnpj"] = "00999888000177",
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
        if (parceiroExiste)
        {
            db.Parceiros.Add(parceiro);
            await db.SaveChangesAsync();
        }
        var environment = new AmbienteTeste { EnvironmentName = ambiente };
        await BootstrapUsuario.ExecutarAsync(provider, config, environment);
        await BootstrapUsuario.ExecutarAsync(provider, config, environment);
        db.ChangeTracker.Clear();
        var usuarios = await db.Users.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(quantidade, usuarios.Count);
        var deveExistirParceiro = parceiroExiste || ambiente == "Development" && habilitado;
        Assert.Equal(deveExistirParceiro ? 1 : 0, await db.Parceiros.CountAsync());
        if (deveExistirParceiro)
        {
            var parceiroCriado = Assert.Single(await db.Parceiros.ToListAsync());
            Assert.Equal("00999888000177", parceiroCriado.Cnpj);
            var cadastroAtualizado = ambiente == "Development" && habilitado;
            Assert.Equal(
                cadastroAtualizado ? "Officio Desenvolvimento" : "Parceiro de teste",
                parceiroCriado.RazaoSocial);
        }
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
