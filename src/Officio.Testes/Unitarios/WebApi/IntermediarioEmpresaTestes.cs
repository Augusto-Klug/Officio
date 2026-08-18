using Microsoft.AspNetCore.Http;
using Officio.Infraestrutura.Identidade;
using Officio.WebApi.Intermediarios;

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

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual());

        Assert.True(chamado);
        Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WebhookD4SignPassaSemTenant()
    {
        var chamado = false;
        var contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/v1/contratos/webhook/d4sign";
        var intermediario = new IntermediarioEmpresa(_ =>
        {
            chamado = true;
            return Task.CompletedTask;
        });

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual());

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

        await intermediario.InvokeAsync(contexto, new ServicoEmpresaAtual());

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
                new System.Security.Claims.Claim("tenant_id", tenantId.ToString())
            ], "Teste"));
        var intermediario = new IntermediarioEmpresa(_ => Task.CompletedTask);

        await intermediario.InvokeAsync(contexto, empresaAtual);

        Assert.Equal(tenantId, empresaAtual.TenantId);
        Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
    }
}
