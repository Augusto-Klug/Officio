using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Entidades;
using Officio.Dominio.Seguranca;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;
using Officio.WebApi.Inicializacao;

namespace Officio.Testes.Unitarios.WebApi;

public class BootstrapAdministradorPlataformaTestes
{
    [Fact]
    public async Task ExecutarAsync_CriaParceiroEPlatformAdminComSenhaHash()
    {
        using var provider = CriarServicos();
        var configuration = CriarConfiguracao();

        await BootstrapAdministradorPlataforma.ExecutarAsync(provider, configuration);

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var parceiro = Assert.Single(await db.Parceiros.IgnoreQueryFilters().ToListAsync());
        var usuario = Assert.Single(await db.Users.IgnoreQueryFilters().ToListAsync());

        Assert.Equal(Guid.Parse(configuration["BootstrapUser:TenantId"]!), parceiro.Id);
        Assert.Equal("00999888000177", parceiro.Cnpj);
        Assert.Equal(PerfisAcesso.PlatformAdmin, usuario.Role);
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<Usuario>()
            .VerifyHashedPassword(usuario, usuario.SenhaHash, "senha-apenas-para-testes"));
        Assert.NotEqual("senha-apenas-para-testes", usuario.SenhaHash);
    }

    [Fact]
    public async Task ExecutarAsync_EIdempotenteENaoRedefineSenha()
    {
        using var provider = CriarServicos();
        var configuration = CriarConfiguracao();
        await BootstrapAdministradorPlataforma.ExecutarAsync(provider, configuration);
        configuration["BootstrapUser:Password"] = "outra-senha";

        await BootstrapAdministradorPlataforma.ExecutarAsync(provider, configuration);

        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var usuario = Assert.Single(await db.Users.IgnoreQueryFilters().ToListAsync());
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<Usuario>()
            .VerifyHashedPassword(usuario, usuario.SenhaHash, "senha-apenas-para-testes"));
        Assert.Single(await db.Parceiros.IgnoreQueryFilters().ToListAsync());
    }

    [Theory]
    [InlineData(false, "PlatformAdmin", "Habilite BootstrapUser:Enabled somente durante a execucao explicita do bootstrap.")]
    [InlineData(true, "Admin", "BootstrapUser:Role deve ser PlatformAdmin para este comando.")]
    public async Task ExecutarAsync_RejeitaConfiguracaoInseguraSemCriarDados(
        bool habilitado,
        string role,
        string mensagemEsperada)
    {
        using var provider = CriarServicos();
        var configuration = CriarConfiguracao(habilitado, role);

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BootstrapAdministradorPlataforma.ExecutarAsync(provider, configuration));

        Assert.Equal(mensagemEsperada, erro.Message);
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        Assert.Empty(await db.Users.IgnoreQueryFilters().ToListAsync());
        Assert.Empty(await db.Parceiros.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task ExecutarAsync_RejeitaEmailJaUsadoPorOutroPerfil()
    {
        using var provider = CriarServicos();
        var configuration = CriarConfiguracao();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var tenantId = Guid.Parse(configuration["BootstrapUser:TenantId"]!);
            db.Parceiros.Add(Parceiro.CriarDesenvolvimento(
                tenantId, "00999888000177", "Officio Plataforma"));
            db.Users.Add(Usuario.Create(tenantId, "Usuario existente", "admin@officio.local", "hash", "Admin"));
            await db.SaveChangesAsync();
        }

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BootstrapAdministradorPlataforma.ExecutarAsync(provider, configuration));

        Assert.Contains("O e-mail ja esta associado", erro.Message);
        using var verificacao = provider.CreateScope();
        var contexto = verificacao.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        Assert.Equal("Admin", (await contexto.Users.IgnoreQueryFilters().SingleAsync()).Role);
    }

    [Fact]
    public async Task ExecutarAsync_RejeitaParceiroExistenteComDadosDiferentesSemAlterarCadastro()
    {
        using var provider = CriarServicos();
        var configuration = CriarConfiguracao();
        var tenantId = Guid.Parse(configuration["BootstrapUser:TenantId"]!);
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            db.Parceiros.Add(Parceiro.CriarDesenvolvimento(
                tenantId, "00999888000177", "Razao social existente"));
            await db.SaveChangesAsync();
        }

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BootstrapAdministradorPlataforma.ExecutarAsync(provider, configuration));

        Assert.Contains("CNPJ ou razao social diferentes", erro.Message);
        using var verificacao = provider.CreateScope();
        var contexto = verificacao.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        Assert.Equal("Razao social existente", (await contexto.Parceiros.IgnoreQueryFilters().SingleAsync()).RazaoSocial);
        Assert.Empty(await contexto.Users.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task ExecutarAsync_RejeitaCnpjJaVinculadoAOutroParceiro()
    {
        using var provider = CriarServicos();
        var configuration = CriarConfiguracao();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            db.Parceiros.Add(Parceiro.Criar("00999888000177", "Outro parceiro"));
            await db.SaveChangesAsync();
        }

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => BootstrapAdministradorPlataforma.ExecutarAsync(provider, configuration));

        Assert.Contains("ja pertence a outro parceiro", erro.Message);
        using var verificacao = provider.CreateScope();
        var contexto = verificacao.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        Assert.Single(await contexto.Parceiros.IgnoreQueryFilters().ToListAsync());
        Assert.Empty(await contexto.Users.IgnoreQueryFilters().ToListAsync());
    }

    private static ServiceProvider CriarServicos()
    {
        var banco = Guid.NewGuid().ToString();
        return new ServiceCollection()
            .AddLogging()
            .AddScoped(_ => new ContextoAplicacao(
                new DbContextOptionsBuilder<ContextoAplicacao>().UseInMemoryDatabase(banco).Options,
                new ServicoEmpresaAtual(), new PublicadorEventosFake()))
            .BuildServiceProvider();
    }

    private static IConfigurationRoot CriarConfiguracao(bool habilitado = true, string role = "PlatformAdmin")
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BootstrapUser:Enabled"] = habilitado.ToString(),
            ["BootstrapUser:TenantId"] = Guid.NewGuid().ToString(),
            ["BootstrapUser:TenantNome"] = "Officio Plataforma",
            ["BootstrapUser:TenantCnpj"] = "00999888000177",
            ["BootstrapUser:TenantNomeFantasia"] = "Officio",
            ["BootstrapUser:Nome"] = "Administrador inicial",
            ["BootstrapUser:Email"] = "admin@officio.local",
            ["BootstrapUser:Password"] = "senha-apenas-para-testes",
            ["BootstrapUser:Role"] = role
        }).Build();
}
