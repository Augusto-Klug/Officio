using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Officio.Infraestrutura.Identidade;
using Officio.WebApi.Intermediarios;
using Officio.Dominio.Interfaces;

namespace Officio.Testes.Unitarios.WebApi;

public class IntermediarioEmpresaTestes
{
    [Fact]
    public async Task InvokeAsync_LoginPassaSemTenant()
    {
        var chamado = false;
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/autenticacao/Login";
        var intermediario = new IntermediarioEmpresa(_ =>
        {
            chamado = true;
            return Task.CompletedTask;
        });

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual(), new VerificadorEmpresaFake());

        Assert.True(chamado);
        Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WebhookAutentiquePassaSemTenant()
    {
        var chamado = false;
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/contratos/webhook/autentique";
        var intermediario = new IntermediarioEmpresa(_ =>
        {
            chamado = true;
            return Task.CompletedTask;
        });

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual(), new VerificadorEmpresaFake());

        Assert.True(chamado);
        Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_FichaAdmissaoPublicaPassaSemTenant()
    {
        var chamado = false;
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/admissoes-publicas/Obter/token";
        var intermediario = new IntermediarioEmpresa(_ =>
        {
            chamado = true;
            return Task.CompletedTask;
        });

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual(), new VerificadorEmpresaFake());

        Assert.True(chamado);
        Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RotaProtegidaSemTenantRetorna401()
    {
        var chamado = false;
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/prospectos";
        var intermediario = new IntermediarioEmpresa(_ =>
        {
            chamado = true;
            return Task.CompletedTask;
        });

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual(), new VerificadorEmpresaFake());

        Assert.False(chamado);
        Assert.Equal(StatusCodes.Status401Unauthorized, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RotaProtegidaComClaimValidaPopulaEmpresaAtual()
    {
        var tenantId = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/prospectos";
        contexto.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
            [
                new System.Security.Claims.Claim("tenant_id", tenantId.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            ], "Teste"));
        var intermediario = new IntermediarioEmpresa(_ => Task.CompletedTask);

        await intermediario.InvokeAsync(contexto, empresaAtual, new VerificadorEmpresaFake());

        Assert.Equal(tenantId, empresaAtual.TenantId);
        Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AdminGlobalPlatformAdminPassaSemTenant()
    {
        var chamado = false;
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/admin/empresas";
        contexto.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Role, "PlatformAdmin"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            ], "Teste"));
        var intermediario = new IntermediarioEmpresa(_ =>
        {
            chamado = true;
            return Task.CompletedTask;
        });

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual(), new VerificadorEmpresaFake());

        Assert.True(chamado);
        Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
    }

    [Theory]
    [InlineData(null, StatusCodes.Status401Unauthorized)]
    [InlineData("Admin", StatusCodes.Status403Forbidden)]
    public async Task InvokeAsync_AdminGlobalBloqueiaAnonimoEAdminTenant(string? perfil, int statusEsperado)
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/admin/empresas";
        if (perfil is not null)
            contexto.User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Role, perfil),
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                ], "Teste"));
        var intermediario = new IntermediarioEmpresa(_ => Task.CompletedTask);

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual(), new VerificadorEmpresaFake());

        Assert.Equal(statusEsperado, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_EmpresaInativaRetorna403()
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/prospectos";
        contexto.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("tenant_id", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            ], "Teste"));

        await new IntermediarioEmpresa(_ => Task.CompletedTask)
            .InvokeAsync(contexto, new ServicoEmpresaAtual(), new VerificadorEmpresaFake(true, false));

        Assert.Equal(StatusCodes.Status403Forbidden, contexto.Response.StatusCode);
    }

    private sealed class VerificadorEmpresaFake(bool usuarioAtivo = true, bool empresaAtiva = true) : IVerificadorAcesso
    {
        public Task<bool> UsuarioAtivoAsync(Guid usuarioId, CancellationToken ct = default)
            => Task.FromResult(usuarioAtivo);

        public Task<bool> EmpresaAtivaAsync(Guid empresaId, CancellationToken ct = default)
            => Task.FromResult(empresaAtiva);
    }
}
