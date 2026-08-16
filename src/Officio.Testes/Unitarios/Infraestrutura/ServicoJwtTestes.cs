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
    public async Task AutenticarAsync_ComSenhaCorretaHashValido_Autentica()
    {
        var tenantId = Guid.NewGuid();
        var usuario = Usuario.Create(tenantId, "Maria Silva", "maria@officio.test", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha-correta"));

        await using var contexto = CriarContexto(tenantId);
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
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();

        var servico = new ServicoJwt(CriarConfiguracao(), contexto);

        var resultado = await servico.AutenticarAsync(usuario.Email, "senha-incorreta");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task AutenticarAsync_TokenContemClaimsEssenciais()
    {
        var tenantId = Guid.NewGuid();
        var usuario = Usuario.Create(tenantId, "Maria Silva", "maria@officio.test", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha-correta"));

        await using var contexto = CriarContexto(tenantId);
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
