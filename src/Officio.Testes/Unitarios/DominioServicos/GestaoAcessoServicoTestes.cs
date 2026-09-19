using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class GestaoAcessoServicoTestes
{
    [Fact]
    public async Task OnboardingEAceite_CriaEmpresaAdminVinculoETokenDeUsoUnico()
    {
        await using var contexto = CriarContexto(Guid.Empty);
        var email = new EmailFake();
        var servico = CriarServico(contexto, new ServicoEmpresaAtual(), new UsuarioAtualFake(Guid.Empty), email);

        var onboarding = await servico.CriarEmpresaAdministradorAsync(new(
            "11.222.333/0001-44",
            "Empresa Teste Ltda",
            "Empresa Teste",
            "contato@empresa.test",
            "Maria Admin",
            "MARIA@EMPRESA.TEST"));

        Assert.True(onboarding.Sucesso);
        var token = email.ObterTokenDoUltimoLink();
        var aceite = await servico.AceitarConviteAsync(new(token, "senha-segura-123"));
        var segundoAceite = await servico.AceitarConviteAsync(new(token, "outra-senha-123"));

        Assert.True(aceite.Sucesso);
        Assert.False(segundoAceite.Sucesso);
        var usuario = Assert.Single(await contexto.Users.IgnoreQueryFilters().ToListAsync());
        Assert.Equal(onboarding.Valor!.EmpresaId, usuario.TenantId);
        Assert.Equal("Admin", usuario.Role);
        Assert.Equal("maria@empresa.test", usuario.Email);
        Assert.Single(await contexto.UsuarioEmpresas.ToListAsync());
    }

    [Fact]
    public async Task ListarUsuarios_RespeitaFiltroDoTenant()
    {
        var tenantId = Guid.NewGuid();
        var outroTenantId = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        await using var contexto = CriarContexto(tenantId);
        contexto.Users.AddRange(
            Usuario.Create(tenantId, "Local", "local@test.local", "hash", "Admin"),
            Usuario.Create(outroTenantId, "Externo", "externo@test.local", "hash", "Admin"));
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, empresaAtual, new UsuarioAtualFake(Guid.NewGuid()), new EmailFake());

        var resultado = await servico.ListarUsuariosAsync();

        var usuario = Assert.Single(resultado.Valor!);
        Assert.Equal("Local", usuario.Nome);
    }

    [Fact]
    public async Task UltimoAdmin_NaoPodeSerDesativadoNemRebaixado()
    {
        var tenantId = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        await using var contexto = CriarContexto(tenantId);
        var admin = Usuario.Create(tenantId, "Admin", "admin@test.local", "hash", "Admin");
        contexto.Users.Add(admin);
        contexto.UsuarioEmpresas.Add(UsuarioEmpresa.Criar(admin.Id, tenantId));
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, empresaAtual, new UsuarioAtualFake(Guid.NewGuid()), new EmailFake());

        var status = await servico.AlterarStatusAsync(admin.Id, false);
        var perfil = await servico.AlterarPerfilAsync(admin.Id, "Comercial");

        Assert.False(status.Sucesso);
        Assert.False(perfil.Sucesso);
        Assert.True(admin.Ativo);
        Assert.Equal("Admin", admin.Role);
    }

    [Fact]
    public async Task RedefinirSenha_InvalidaTokenDepoisDoPrimeiroUso()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(Guid.Empty);
        var usuario = Usuario.Create(tenantId, "Maria", "maria@test.local", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha-antiga"));
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();
        var email = new EmailFake();
        var servico = CriarServico(contexto, new ServicoEmpresaAtual(), new UsuarioAtualFake(Guid.Empty), email);

        await servico.SolicitarRedefinicaoSenhaAsync(usuario.Email);
        var token = email.ObterTokenDoUltimoLink();
        var primeira = await servico.RedefinirSenhaAsync(new(token, "senha-nova-123"));
        var segunda = await servico.RedefinirSenhaAsync(new(token, "senha-outra-123"));

        Assert.True(primeira.Sucesso);
        Assert.False(segunda.Sucesso);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<Usuario>().VerifyHashedPassword(usuario, usuario.SenhaHash, "senha-nova-123"));
    }

    [Fact]
    public async Task AdministracaoPlataforma_ListaESuspendeEmpresaEReenviaConvite()
    {
        await using var contexto = CriarContexto(Guid.Empty);
        var email = new EmailFake();
        var servico = CriarServico(contexto, new ServicoEmpresaAtual(), new UsuarioAtualFake(Guid.Empty), email);
        var onboarding = await servico.CriarEmpresaAdministradorAsync(new(
            "11222333000144", "Empresa Teste Ltda", null, null, "Maria Admin", "maria@empresa.test"));

        var lista = await servico.ListarEmpresasAsync();
        var empresa = Assert.Single(lista.Valor!);
        Assert.Equal(onboarding.Valor!.EmpresaId, empresa.Id);
        Assert.Equal("maria@empresa.test", empresa.AdministradorEmail);
        Assert.True(empresa.Ativa);

        Assert.True((await servico.AlterarStatusEmpresaAsync(empresa.Id, false)).Sucesso);
        Assert.False((await contexto.Empresas.FindAsync(empresa.Id))!.Ativo);
        Assert.True((await servico.ReenviarConviteAdministradorAsync(empresa.Id)).Sucesso);
        Assert.Equal(2, email.Quantidade);
    }

    private static GestaoAcessoServico CriarServico(
        ContextoAplicacao contexto,
        IEmpresaAtual empresaAtual,
        IUsuarioAtual usuarioAtual,
        IServicoEmail email)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Acesso:FrontendBaseUrl"] = "http://localhost:5173",
            ["Acesso:ConviteExpiracaoHoras"] = "48",
            ["Acesso:RedefinicaoSenhaExpiracaoHoras"] = "1"
        }).Build();
        return new GestaoAcessoServico(
            new GestaoAcessoRepositorio(contexto),
            empresaAtual,
            usuarioAtual,
            new ServicoSenha(),
            email,
            config);
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        if (tenantId != Guid.Empty)
            empresaAtual.SetTenant(tenantId);
        return new ContextoAplicacao(
            new DbContextOptionsBuilder<ContextoAplicacao>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            empresaAtual,
            new PublicadorEventosFake());
    }

    private sealed class UsuarioAtualFake(Guid userId) : IUsuarioAtual
    {
        public Guid UserId { get; } = userId;
        public string? Email => null;
    }

    private sealed class EmailFake : IServicoEmail
    {
        private readonly List<string> _corpos = [];
        public int Quantidade => _corpos.Count;

        public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            _corpos.Add(body);
            return Task.CompletedTask;
        }

        public string ObterTokenDoUltimoLink()
        {
            var corpo = Assert.Single(_corpos.TakeLast(1));
            var indice = corpo.LastIndexOf("token=", StringComparison.Ordinal);
            Assert.True(indice >= 0);
            return Uri.UnescapeDataString(corpo[(indice + 6)..]);
        }
    }
}
