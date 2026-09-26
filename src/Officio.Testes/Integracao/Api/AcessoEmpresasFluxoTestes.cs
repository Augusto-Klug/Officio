using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class AcessoEmpresasFluxoTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task IsolamentoTenant_ImpedeAlterarProspectoDeOutraEmpresa()
    {
        await factory.PrepararTenantAsync();
        using var cliente = factory.CreateClient();
        var tokenPrimeiroTenant = await LoginAsync(cliente, "admin@integracao.test", "Tenant@123");
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenPrimeiroTenant);

        var criacao = await cliente.PostAsJsonAsync("/api/v1/prospectos/Criar", new
        {
            razaoSocial = "Prospecto Restrito Ltda",
            cnpj = "44.555.666/0001-77"
        });
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        var prospectoId = Guid.Parse(
            (await criacao.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<string>());

        await factory.AdicionarTenantAsync(
            "88777666000155",
            "admin@segundo-tenant.test",
            "Segundo@123");
        var tokenSegundoTenant = await LoginAsync(cliente, "admin@segundo-tenant.test", "Segundo@123");
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenSegundoTenant);

        var tentativa = await cliente.PostAsync(
            $"/api/v1/prospectos/ConverterParaCliente/{prospectoId}",
            null);

        Assert.True(
            tentativa.StatusCode == HttpStatusCode.NotFound,
            $"Isolamento retornou {(int)tentativa.StatusCode}: {await tentativa.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task FluxoComercial_CriaProspectoOrcamentoAceitoEContrato()
    {
        var tenantId = await factory.PrepararTenantAsync();
        using var cliente = factory.CreateClient();
        var token = await LoginAsync(cliente, "admin@integracao.test", "Tenant@123");
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criacaoProspecto = await cliente.PostAsJsonAsync("/api/v1/prospectos/Criar", new
        {
            razaoSocial = "Cliente Integracao Ltda",
            cnpj = "44.555.666/0001-77",
            regimeTributario = 0,
            socios = new[]
            {
                new { nome = "Socio Integracao", cpf = "117.892.999-00", participacao = 0.1m }
            }
        });
        Assert.Equal(HttpStatusCode.Created, criacaoProspecto.StatusCode);
        var prospectoId = Guid.Parse(
            (await criacaoProspecto.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<string>());

        var criacaoOrcamento = await cliente.PostAsJsonAsync("/api/v1/orcamentos/Criar", new
        {
            prospectoId,
            titulo = "Proposta mensal",
            diaVencimento = 10,
            observacoes = "Fluxo de integracao HTTP",
            itens = new[]
            {
                new
                {
                    descricao = "Honorarios contabeis",
                    secao = 2,
                    tipo = 2,
                    naturezaCobranca = 2,
                    quantidade = 1,
                    valorUnitario = 500m,
                    ordem = 1
                }
            }
        });
        Assert.Equal(HttpStatusCode.Created, criacaoOrcamento.StatusCode);
        var orcamentoId = await criacaoOrcamento.Content.ReadFromJsonAsync<Guid>();

        var aceite = await cliente.PostAsync($"/api/v1/orcamentos/Aceitar/{orcamentoId}", null);
        Assert.Equal(HttpStatusCode.OK, aceite.StatusCode);

        var conversao = await cliente.PostAsJsonAsync($"/api/v1/orcamentos/ConverterParaContrato/{orcamentoId}", new
        {
            dataInicio = new DateTime(2026, 10, 1),
            dataFim = (DateTime?)null,
            renovacaoAutomatica = true,
            clausulasExtras = "Contrato criado pelo teste de integracao."
        });
        Assert.True(
            conversao.StatusCode == HttpStatusCode.Created,
            $"Conversao retornou {(int)conversao.StatusCode}: {await conversao.Content.ReadAsStringAsync()}");
        var contratoId = await conversao.Content.ReadFromJsonAsync<Guid>();

        var detalheContrato = await cliente.GetAsync($"/api/v1/contratos/ObterPorId/{contratoId}");
        Assert.Equal(HttpStatusCode.OK, detalheContrato.StatusCode);
        var contrato = await detalheContrato.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Cliente Integracao Ltda", contrato!["razaoSocial"]!.GetValue<string>());
        Assert.Equal("Rascunho", contrato["status"]!.GetValue<string>());

        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var auditorias = await contexto.AuditLogs.Where(item => item.TenantId == tenantId).ToListAsync();
        Assert.Contains(auditorias, item => item.Acao == "CriarProspectoComando:CRIADO");
        Assert.Contains(auditorias, item => item.Acao == "CriarOrcamentoComando:CRIADO");
        Assert.Contains(auditorias, item => item.Acao == "ConverterOrcamentoParaContratoComando:CRIADO");
    }

    [Fact]
    public async Task FluxoCompleto_CriaEmpresaAtivaAdministradorEPermiteAcessoAoTenant()
    {
        await factory.ReiniciarBancoAsync();
        using var cliente = factory.CreateClient();

        var tokenPlataforma = await LoginAsync(cliente, "plataforma@officio.test", "Senha@123");
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenPlataforma);

        var criacao = await cliente.PostAsJsonAsync("/api/v1/admin/parceiros", new
        {
            cnpj = "11.222.333/0001-44",
            razaoSocial = "Empresa Integracao Ltda",
            nomeFantasia = "Empresa Integracao",
            emailParceiro = "contato@empresa.test",
            nomeAdministrador = "Administrador Tenant",
            emailAdministrador = "admin@empresa.test"
        });

        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        var parceiroId = Guid.Parse((await criacao.Content.ReadFromJsonAsync<JsonObject>())!["parceiroId"]!.GetValue<string>());
        var emailConvite = Assert.Single(factory.ServicoEmail.Emails);
        var tokenConvite = ExtrairToken(emailConvite.Corpo);

        cliente.DefaultRequestHeaders.Authorization = null;
        var ativacao = await cliente.PostAsJsonAsync("/api/v1/autenticacao/AceitarConvite", new
        {
            token = tokenConvite,
            senha = "Tenant@123"
        });
        Assert.Equal(HttpStatusCode.OK, ativacao.StatusCode);

        var tokenTenant = await LoginAsync(cliente, "admin@empresa.test", "Tenant@123");
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenTenant);
        var usuarios = await cliente.GetAsync("/api/v1/usuarios");

        Assert.Equal(HttpStatusCode.OK, usuarios.StatusCode);
        var listaUsuarios = await usuarios.Content.ReadFromJsonAsync<JsonArray>();
        Assert.Contains(listaUsuarios!, item => item!["email"]!.GetValue<string>() == "admin@empresa.test");

        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var parceiro = await contexto.Parceiros.SingleAsync(item => item.Id == parceiroId);
        var auditorias = await contexto.AuditLogs.Where(item => item.TenantId == parceiroId).ToListAsync();

        Assert.Equal("11222333000144", parceiro.Cnpj);
        Assert.Empty(await contexto.Empresas.IgnoreQueryFilters().ToListAsync());
        Assert.Contains(auditorias, item => item.Acao == "CriarParceiroAdministradorComando:CRIADO");
        Assert.Contains(auditorias, item => item.Acao == "AceitarConviteComando:CRIADO");
    }

    private static async Task<string> LoginAsync(HttpClient cliente, string email, string senha)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new { email, senha });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
    }

    private static string ExtrairToken(string corpo)
    {
        const string marcador = "token=";
        var inicio = corpo.IndexOf(marcador, StringComparison.Ordinal);
        Assert.True(inicio >= 0, "O e-mail de convite nao contem o token de ativacao.");
        return Uri.UnescapeDataString(corpo[(inicio + marcador.Length)..]);
    }
}
