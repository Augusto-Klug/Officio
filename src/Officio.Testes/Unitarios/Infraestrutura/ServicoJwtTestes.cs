using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class ServicoJwtTestes
{
    [Fact]
    public async Task AutenticarAsync_UsuarioInexistente_Falha()
    {
        await using var contexto = CriarContexto(Guid.NewGuid());
        Assert.Null(await new ServicoJwt(CriarConfiguracao(), contexto).AutenticarAsync("ausente@test.local", "senha"));
    }

    [Fact]
    public async Task AutenticarAsync_UsuarioInativo_Falha()
    {
        await using var contexto = CriarContexto(Guid.NewGuid());
        var usuario = Usuario.Create(Guid.NewGuid(), "Maria", "maria@test.local", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha"));
        contexto.Parceiros.Add(Parceiro.CriarDesenvolvimento(usuario.TenantId, "00999888000177", "Parceiro Teste"));
        contexto.Users.Add(usuario);
        contexto.Entry(usuario).Property(u => u.Ativo).CurrentValue = false;
        await contexto.SaveChangesAsync();
        Assert.Null(await new ServicoJwt(CriarConfiguracao(), contexto).AutenticarAsync(usuario.Email, "senha"));
    }

    [Theory]
    [InlineData(null, 8)]
    [InlineData("2", 2)]
    public async Task AutenticarAsync_NormalizaEmailSemTenantEUsaExpiracaoConfigurada(string? horas, int esperado)
    {
        await using var contexto = CriarContexto(Guid.Empty);
        var usuario = Usuario.Create(Guid.NewGuid(), "Maria", "maria@test.local", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha"));
        contexto.Parceiros.Add(Parceiro.CriarDesenvolvimento(usuario.TenantId, "00999888000177", "Parceiro Teste"));
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();
        var config = CriarConfiguracao();
        config["Jwt:ExpirationHours"] = horas;
        var antes = DateTime.UtcNow;
        var resultado = await new ServicoJwt(config, contexto).AutenticarAsync("  maria@test.local  ", "senha");
        Assert.NotNull(resultado);
        Assert.InRange(resultado.ExpiraEm, antes.AddHours(esperado), DateTime.UtcNow.AddHours(esperado));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(resultado.Token);
        Assert.InRange((resultado.ExpiraEm - jwt.ValidTo).TotalSeconds, 0, 1);
    }

    [Fact]
    public void ConfiguracaoSemChave_FalhaClaramente()
    {
        var config = CriarConfiguracao();
        config["Jwt:Key"] = null;
        var erro = Assert.Throws<InvalidOperationException>(() =>
            Officio.WebApi.Extensoes.ExtensoesServiceCollection.AdicionarServicosOfficio(
                new Microsoft.Extensions.DependencyInjection.ServiceCollection(), config));
        Assert.Contains("Jwt:Key", erro.Message);
    }

    [Fact]
    public async Task AutenticarAsync_ComSenhaCorretaHashValido_Autentica()
    {
        var tenantId = Guid.NewGuid();
        var usuario = Usuario.Create(tenantId, "Maria Silva", "maria@officio.test", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha-correta"));

        await using var contexto = CriarContexto(tenantId);
        contexto.Parceiros.Add(Parceiro.CriarDesenvolvimento(tenantId, "00999888000177", "Parceiro Teste"));
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();

        var servico = new ServicoJwt(CriarConfiguracao(), contexto);

        var resultado = await servico.AutenticarAsync(usuario.Email, "senha-correta");

        Assert.NotNull(resultado);
        Assert.Equal(usuario.Id, resultado!.Usuario.Id);
    }

    [Fact]
    public async Task AutenticarAsync_ComSenhaIncorreta_Falha()
    {
        var tenantId = Guid.NewGuid();
        var usuario = Usuario.Create(tenantId, "Maria Silva", "maria@officio.test", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha-correta"));

        await using var contexto = CriarContexto(tenantId);
        contexto.Parceiros.Add(Parceiro.CriarDesenvolvimento(tenantId, "00999888000177", "Parceiro Teste"));
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();

        var servico = new ServicoJwt(CriarConfiguracao(), contexto);

        var resultado = await servico.AutenticarAsync(usuario.Email, "senha-incorreta");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AutenticarAsync_ParceiroSuspenso_Falha()
    {
        var parceiroId = Guid.NewGuid();
        var parceiro = Parceiro.CriarDesenvolvimento(parceiroId, "00999888000177", "Parceiro suspenso");
        parceiro.AlterarStatus(false);
        var usuario = Usuario.Create(parceiro.Id, "Maria", "maria@suspensa.test", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha-correta"));
        await using var contexto = CriarContexto(Guid.Empty);
        contexto.Parceiros.Add(parceiro);
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();

        var resultado = await new ServicoJwt(CriarConfiguracao(), contexto)
            .AutenticarAsync(usuario.Email, "senha-correta");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AutenticarAsync_TokenContemClaimsEssenciais()
    {
        var tenantId = Guid.NewGuid();
        var usuario = Usuario.Create(tenantId, "Maria Silva", "maria@officio.test", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha-correta"));

        await using var contexto = CriarContexto(tenantId);
        contexto.Parceiros.Add(Parceiro.CriarDesenvolvimento(tenantId, "00999888000177", "Parceiro Teste"));
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();

        var servico = new ServicoJwt(CriarConfiguracao(), contexto);

        var resultado = await servico.AutenticarAsync(usuario.Email, "senha-correta");

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(resultado!.Token);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == usuario.Id.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == usuario.Id.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == usuario.Email);
        Assert.Contains(jwt.Claims, c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
        Assert.Contains(jwt.Claims, c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == usuario.Role);
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);

        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }

    private static IConfiguration CriarConfiguracao()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "chave-super-secreta-para-testes-officio",
                ["Jwt:Issuer"] = "Officio.Testes",
                ["Jwt:Audience"] = "Officio.Testes"
            })
            .Build();
}
