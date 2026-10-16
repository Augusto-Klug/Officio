using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class ProspectoCadastroFluxoTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task Inicial_EditaDadosESociosDepoisExcluiComAuditoria()
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var cadastro = await ObterDadosAsync(cliente, id);
        var socio = cadastro["socios"]![0]!.AsObject();
        var socioId = socio["id"]!.GetValue<Guid>();
        cadastro["razaoSocial"] = "Empresa revisada";
        cadastro["cnpj"] = "99.093.538/0001-55";
        cadastro["regimeTributario"] = 2;
        cadastro["email"] = "novo@empresa.test";
        cadastro["telefone"] = "47999998888";
        cadastro["inscricaoEstadual"] = "12345";
        socio["nome"] = "Socio revisado";
        socio["participacao"] = 70;
        cadastro["socios"]!.AsArray().Add(new JsonObject { ["nome"] = "Novo socio", ["participacao"] = 30 });

        var atualizacao = await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", cadastro);
        Assert.Equal(HttpStatusCode.NoContent, atualizacao.StatusCode);
        var revisado = await ObterDadosAsync(cliente, id);
        Assert.Equal("Empresa revisada", revisado["razaoSocial"]!.GetValue<string>());
        Assert.Equal("99093538000155", revisado["cnpj"]!.GetValue<string>());
        Assert.Equal(2, revisado["regimeTributario"]!.GetValue<int>());
        Assert.Equal("novo@empresa.test", revisado["email"]!.GetValue<string>());
        Assert.Equal("12345", revisado["inscricaoEstadual"]!.GetValue<string>());
        Assert.Equal(2, revisado["socios"]!.AsArray().Count);
        Assert.Contains(revisado["socios"]!.AsArray(), s => s!["id"]!.GetValue<Guid>() == socioId);

        revisado["socios"] = new JsonArray();
        Assert.Equal(HttpStatusCode.NoContent,
            (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", revisado)).StatusCode);
        Assert.Empty((await ObterDadosAsync(cliente, id))["socios"]!.AsArray());

        var exclusao = await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}");
        Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        Assert.Empty(await db.Socios.IgnoreQueryFilters().Where(s => s.ProspectId == id).ToListAsync());
        var auditorias = await db.AuditLogs.IgnoreQueryFilters().Select(a => a.Acao).ToListAsync();
        Assert.Contains("AtualizarProspectoComando:ALTERADO", auditorias);
        Assert.Contains("ExcluirProspectoComando:REMOVIDO", auditorias);
    }

    [Theory]
    [InlineData(StatusProspecto.PropostaEnviada)]
    [InlineData(StatusProspecto.Convertido)]
    [InlineData(StatusProspecto.Perdido)]
    [InlineData(StatusProspecto.Inativo)]
    public async Task ForaDaEtapaInicial_RejeitaEditarEExcluir(StatusProspecto status)
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var dados = await ObterDadosAsync(cliente, id);
        dados["razaoSocial"] = "Nao pode alterar";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            (await db.Prospects.IgnoreQueryFilters().SingleAsync(p => p.Id == id)).UpdateStatus(status);
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Conflict, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        Assert.Equal("Empresa inicial", (await ObterDadosAsync(cliente, id))["razaoSocial"]!.GetValue<string>());
    }

    [Fact]
    public async Task OutroParceiro_NaoObtemEditaOuExcluiCadastro()
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var dados = await ObterDadosAsync(cliente, id);
        await factory.AdicionarTenantAsync("88777666000155", "outro@integracao.test", "Outro@123");
        using var outro = await ClienteAutenticadoAsync("outro@integracao.test", "Outro@123");

        Assert.Equal(HttpStatusCode.NotFound, (await outro.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outro.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outro.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
    }

    [Fact]
    public async Task Editar_RejeitaCnpjDuplicadoCamposInvalidosESocioDeOutroCadastro()
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var primeiro = await CriarAsync(cliente);
        var segundo = await CriarAsync(cliente, "11222333000144");
        var dados = await ObterDadosAsync(cliente, primeiro);
        dados["cnpj"] = "11.222.333/0001-44";
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{primeiro}", dados)).StatusCode);
        dados["cnpj"] = "12";
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{primeiro}", dados)).StatusCode);
        dados["cnpj"] = "99093538000155";
        dados["socios"] = (await ObterDadosAsync(cliente, segundo))["socios"]!.DeepClone();
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{primeiro}", dados)).StatusCode);
        Assert.Equal("99093538000155", (await ObterDadosAsync(cliente, primeiro))["cnpj"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Exclusao_ComVinculos_RejeitaSemApagarHistorico(bool documento)
    {
        var tenant = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            if (documento)
                db.Documents.Add(Documento.Create(tenant, "arquivo.pdf", TipoDocumento.CartaoCnpj, "storage/arquivo.pdf", 1m, prospectId: id));
            else
                db.Budgets.Add(Orcamento.Criar(tenant, id, "Orcamento vinculado", 10, null));
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Conflict, (await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
    }

    [Fact]
    public async Task RemoverSocio_ComDocumentoVinculado_RejeitaSemAlterarCadastro()
    {
        var tenant = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var dados = await ObterDadosAsync(cliente, id);
        var socioId = dados["socios"]![0]!["id"]!.GetValue<Guid>();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            db.Documents.Add(Documento.Create(tenant, "identidade.pdf", TipoDocumento.RgSocio, "storage/rg.pdf", 1m, socioId: socioId));
            await db.SaveChangesAsync();
        }
        dados["razaoSocial"] = "Nao deve salvar";
        dados["socios"] = new JsonArray();
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        var atual = await ObterDadosAsync(cliente, id);
        Assert.Equal("Empresa inicial", atual["razaoSocial"]!.GetValue<string>());
        Assert.Single(atual["socios"]!.AsArray());
    }

    private static async Task<Guid> CriarAsync(HttpClient cliente, string cnpj = "99093538000155")
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/prospectos/Criar", new
        {
            razaoSocial = "Empresa inicial", cnpj, regimeTributario = 0,
            socios = new[] { new { nome = "Socio inicial", cpf = "11789299900", participacao = 100 } }
        });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<Guid>();
    }

    private static async Task<JsonObject> ObterDadosAsync(HttpClient cliente, Guid id)
    {
        var resposta = await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["dados"]!.AsObject();
    }

    private async Task<HttpClient> ClienteAutenticadoAsync(string email = "admin@integracao.test", string senha = "Tenant@123")
    {
        var cliente = factory.CreateClient();
        var login = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new { email, senha });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
