using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Seguranca;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public sealed class PortalClienteApiTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task ClienteAcessaSomentePortalEEmpresasVinculadas()
    {
        var tenantId = await factory.PrepararTenantAsync();
        var (empresaVinculadaId, empresaNaoVinculadaId) = await PrepararUsuarioClienteAsync(tenantId);
        using var cliente = factory.CreateClient();
        var token = await LoginAsync(cliente);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var visao = await cliente.GetAsync("/api/v1/portal-cliente/ObterVisao");
        Assert.Equal(HttpStatusCode.OK, visao.StatusCode);
        var empresas = (await visao.Content.ReadFromJsonAsync<JsonObject>())!["empresas"]!.AsArray();
        var empresa = Assert.Single(empresas);
        Assert.Equal(empresaVinculadaId.ToString(), empresa!["id"]!.GetValue<string>());

        var visaoOutraEmpresa = await cliente.GetAsync(
            $"/api/v1/portal-cliente/ObterVisao?empresaId={empresaNaoVinculadaId}");
        Assert.Equal(HttpStatusCode.Forbidden, visaoOutraEmpresa.StatusCode);

        var detalheOutraEmpresa = await cliente.GetAsync(
            $"/api/v1/portal-cliente/ObterAdmissao/{empresaNaoVinculadaId}/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, detalheOutraEmpresa.StatusCode);

        var rotaInterna = await cliente.GetAsync("/api/v1/admissoes/Listar");
        Assert.Equal(HttpStatusCode.Forbidden, rotaInterna.StatusCode);

        var contratosInternos = await cliente.GetAsync("/api/v1/contratos/Listar");
        Assert.Equal(HttpStatusCode.Forbidden, contratosInternos.StatusCode);
    }

    [Fact]
    public async Task ClienteCriaAdmissaoInformandoSomenteEmailDoFuncionario()
    {
        var tenantId = await factory.PrepararTenantAsync();
        var (empresaId, _) = await PrepararUsuarioClienteAsync(tenantId);
        using var cliente = factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await LoginAsync(cliente));

        var resposta = await cliente.PostAsJsonAsync("/api/v1/portal-cliente/CriarAdmissao", new
        {
            empresaId,
            emailFuncionario = "nova.funcionaria@integracao.test"
        });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var criada = (await resposta.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.Equal("nova.funcionaria@integracao.test", criada["emailFuncionario"]!.GetValue<string>());
        Assert.True(criada["linkFuncionarioEnviado"]!.GetValue<bool>());
        var email = Assert.Single(factory.ServicoEmail.Emails);
        Assert.Equal("nova.funcionaria@integracao.test", email.Destinatario);

        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IEmpresaAtual>().SetTenant(tenantId);
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var admissao = await contexto.Admissoes
            .Include(item => item.StatusAdmissao)
            .SingleAsync(item => item.Id == criada["id"]!.GetValue<Guid>());
        Assert.Equal("Aguardando preenchimento", admissao.NomeFuncionario);
        Assert.Null(admissao.Cpf);
        Assert.Equal("nova.funcionaria@integracao.test", admissao.Email);
        Assert.Equal("AGUARDANDO_ADMITIDO", admissao.StatusAdmissao.Codigo);

        var repositorio = scope.ServiceProvider.GetRequiredService<IAdmissaoRepositorio>();
        var statusEmpregador = await repositorio.ObterOuCriarStatusAsync(
            "AGUARDANDO_EMPREGADOR", "Aguardando empregador", 5);
        admissao.MarcarPreenchimentoFuncionarioConcluido();
        admissao.AlterarStatus(statusEmpregador.Id);
        await contexto.SaveChangesAsync();

        var detalhe = await cliente.GetAsync(
            $"/api/v1/portal-cliente/ObterAdmissao/{empresaId}/{admissao.Id}");
        Assert.Equal(HttpStatusCode.OK, detalhe.StatusCode);
        var detalheJson = (await detalhe.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.True(detalheJson["preenchimentoFuncionarioConcluido"]!.GetValue<bool>());
        Assert.False(detalheJson["preenchimentoEmpresaConcluido"]!.GetValue<bool>());
    }

    private async Task<(Guid Vinculada, Guid NaoVinculada)> PrepararUsuarioClienteAsync(Guid tenantId)
    {
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var empresaVinculada = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa vinculada");
        var empresaNaoVinculada = Empresa.CriarConstituida(tenantId, "22333444000155", "Outra empresa");
        var usuario = Usuario.Create(tenantId, "Usuario Cliente", "cliente@integracao.test", string.Empty, PerfisAcesso.Cliente);
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "Cliente@123"));
        contexto.Empresas.AddRange(empresaVinculada, empresaNaoVinculada);
        contexto.Users.Add(usuario);
        contexto.UsuarioEmpresas.Add(UsuarioEmpresa.Criar(usuario.Id, empresaVinculada.Id));
        await contexto.SaveChangesAsync();
        return (empresaVinculada.Id, empresaNaoVinculada.Id);
    }

    private static async Task<string> LoginAsync(HttpClient cliente)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new
        {
            email = "cliente@integracao.test",
            senha = "Cliente@123"
        });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
    }
}
