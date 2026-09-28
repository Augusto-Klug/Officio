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
    public async Task OnboardingEAceite_CriaParceiroEAdminComTokenDeUsoUnico()
    {
        await using var contexto = CriarContexto(Guid.Empty);
        var email = new EmailFake();
        var servico = CriarServico(contexto, new ServicoEmpresaAtual(), new UsuarioAtualFake(Guid.Empty), email);

        var onboarding = await servico.CriarParceiroAdministradorAsync(new(
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
        Assert.Equal(onboarding.Valor!.ParceiroId, usuario.TenantId);
        Assert.Equal("Admin", usuario.Role);
        Assert.Equal("maria@empresa.test", usuario.Email);
        Assert.Single(await contexto.Parceiros.ToListAsync());
        Assert.Empty(await contexto.Empresas.IgnoreQueryFilters().ToListAsync());
        Assert.Empty(await contexto.UsuarioEmpresas.ToListAsync());
        var emailConvite = Assert.Single(email.Mensagens);
        Assert.Equal("Convite para acessar o Officio", emailConvite.Assunto);
        Assert.Contains("expira em 48 horas", emailConvite.Corpo);
        Assert.Contains("http://localhost:5173/ativar-conta?token=", emailConvite.Corpo);
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
        var emailRedefinicao = Assert.Single(email.Mensagens);
        Assert.Equal("Redefinicao de senha do Officio", emailRedefinicao.Assunto);
        Assert.Contains("expira em 1 hora", emailRedefinicao.Corpo);
        Assert.Contains("http://localhost:5173/redefinir-senha?token=", emailRedefinicao.Corpo);
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
    public async Task AdministracaoPlataforma_ListaESuspendeParceiroEReenviaConvite()
    {
        await using var contexto = CriarContexto(Guid.Empty);
        var email = new EmailFake();
        var servico = CriarServico(contexto, new ServicoEmpresaAtual(), new UsuarioAtualFake(Guid.Empty), email);
        var onboarding = await servico.CriarParceiroAdministradorAsync(new(
            "11222333000144", "Empresa Teste Ltda", null, null, "Maria Admin", "maria@empresa.test"));

        var lista = await servico.ListarParceirosAsync();
        var parceiro = Assert.Single(lista.Valor!);
        Assert.Equal(onboarding.Valor!.ParceiroId, parceiro.Id);
        Assert.Equal("maria@empresa.test", parceiro.AdministradorEmail);
        Assert.True(parceiro.Ativo);

        Assert.True((await servico.AlterarStatusParceiroAsync(parceiro.Id, false)).Sucesso);
        Assert.False((await contexto.Parceiros.FindAsync(parceiro.Id))!.Ativo);
        Assert.True((await servico.ReenviarConviteAdministradorAsync(parceiro.Id)).Sucesso);
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
        public List<(string Destinatario, string Assunto, string Corpo)> Mensagens { get; } = [];
        public int Quantidade => Mensagens.Count;

        public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            Mensagens.Add((to, subject, body));
            return Task.CompletedTask;
        }

        public string ObterTokenDoUltimoLink()
        {
            var corpo = Assert.Single(Mensagens.TakeLast(1)).Corpo;
            var indice = corpo.LastIndexOf("token=", StringComparison.Ordinal);
            Assert.True(indice >= 0);
            return Uri.UnescapeDataString(corpo[(indice + 6)..]);
        }
    }
}
