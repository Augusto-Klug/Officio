using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public sealed class OrcamentoAceiteApiTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task Aceitar_SemOutroOrcamentoAceito_AceitaSemConfirmacao()
    {
        var tenantId = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var cenario = await CriarOrcamentosAsync(tenantId);

        var resposta = await AceitarAsync(cliente, cenario.NovoId, confirmarSubstituicao: false);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var status = await ObterStatusAsync(cenario.AnteriorId, cenario.NovoId);
        Assert.Equal(StatusOrcamento.Rascunho, status[cenario.AnteriorId]);
        Assert.Equal(StatusOrcamento.Aceito, status[cenario.NovoId]);
    }

    [Fact]
    public async Task Aceitar_ComOutroAceito_RetornaCodigoDeConfirmacaoEConfirmaSubstituicao()
    {
        var tenantId = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var cenario = await CriarOrcamentosAsync(tenantId, anteriorAceito: true);

        var solicitacao = await AceitarAsync(cliente, cenario.NovoId, confirmarSubstituicao: false);

        Assert.Equal(HttpStatusCode.Conflict, solicitacao.StatusCode);
        var erro = await solicitacao.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(OrcamentoCodigosErro.AceiteRequerConfirmacao, erro!["code"]!.GetValue<string>());
        var antesDaConfirmacao = await ObterStatusAsync(cenario.AnteriorId, cenario.NovoId);
        Assert.Equal(StatusOrcamento.Aceito, antesDaConfirmacao[cenario.AnteriorId]);
        Assert.Equal(StatusOrcamento.Rascunho, antesDaConfirmacao[cenario.NovoId]);

        var confirmacao = await AceitarAsync(cliente, cenario.NovoId, confirmarSubstituicao: true);

        Assert.Equal(HttpStatusCode.OK, confirmacao.StatusCode);
        var depoisDaConfirmacao = await ObterStatusAsync(cenario.AnteriorId, cenario.NovoId);
        Assert.Equal(StatusOrcamento.Substituido, depoisDaConfirmacao[cenario.AnteriorId]);
        Assert.Equal(StatusOrcamento.Aceito, depoisDaConfirmacao[cenario.NovoId]);
    }

    [Fact]
    public async Task Aceitar_RevalidaContratoVinculadoDepoisDaSolicitacaoDeConfirmacao()
    {
        var tenantId = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var cenario = await CriarOrcamentosAsync(tenantId, anteriorAceito: true);

        var solicitacao = await AceitarAsync(cliente, cenario.NovoId, confirmarSubstituicao: false);
        Assert.Equal(HttpStatusCode.Conflict, solicitacao.StatusCode);
        var erroSolicitacao = await solicitacao.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(OrcamentoCodigosErro.AceiteRequerConfirmacao, erroSolicitacao!["code"]!.GetValue<string>());

        await VincularContratoAsync(cenario.AnteriorId, cenario.ProspectoId);

        var confirmacao = await AceitarAsync(cliente, cenario.NovoId, confirmarSubstituicao: true);

        Assert.Equal(HttpStatusCode.BadRequest, confirmacao.StatusCode);
        var erroConfirmacao = await confirmacao.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(OrcamentoCodigosErro.SubstituicaoBloqueadaContrato, erroConfirmacao!["code"]!.GetValue<string>());
        var status = await ObterStatusAsync(cenario.AnteriorId, cenario.NovoId);
        Assert.Equal(StatusOrcamento.Aceito, status[cenario.AnteriorId]);
        Assert.Equal(StatusOrcamento.Rascunho, status[cenario.NovoId]);
    }

    [Fact]
    public async Task Aceitar_OrcamentoAnteriorComContratoBloqueiaSemSolicitarConfirmacao()
    {
        var tenantId = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var cenario = await CriarOrcamentosAsync(tenantId, anteriorAceito: true);
        await VincularContratoAsync(cenario.AnteriorId, cenario.ProspectoId);

        var resposta = await AceitarAsync(cliente, cenario.NovoId, confirmarSubstituicao: false);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var erro = await resposta.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(OrcamentoCodigosErro.SubstituicaoBloqueadaContrato, erro!["code"]!.GetValue<string>());
        var status = await ObterStatusAsync(cenario.AnteriorId, cenario.NovoId);
        Assert.Equal(StatusOrcamento.Aceito, status[cenario.AnteriorId]);
        Assert.Equal(StatusOrcamento.Rascunho, status[cenario.NovoId]);
    }

    private async Task<(Guid ProspectoId, Guid AnteriorId, Guid NovoId)> CriarOrcamentosAsync(
        Guid tenantId,
        bool anteriorAceito = false)
    {
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var prospecto = Prospecto.Create(tenantId, "Empresa teste aceite", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta atual", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        if (anteriorAceito)
            anterior.Aceitar();

        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        await contexto.SaveChangesAsync();
        return (prospecto.Id, anterior.Id, novo.Id);
    }

    private async Task VincularContratoAsync(Guid orcamentoId, Guid prospectoId)
    {
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var prospecto = await contexto.Prospects.IgnoreQueryFilters().SingleAsync(item => item.Id == prospectoId);
        var orcamento = await contexto.Budgets.IgnoreQueryFilters().SingleAsync(item => item.Id == orcamentoId);
        contexto.Contracts.Add(Contrato.CriarDeOrcamento(orcamento, prospecto, DateTime.Today, null, null));
        await contexto.SaveChangesAsync();
    }

    private async Task<Dictionary<Guid, StatusOrcamento>> ObterStatusAsync(Guid anteriorId, Guid novoId)
    {
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        return await contexto.Budgets.IgnoreQueryFilters()
            .Where(item => item.Id == anteriorId || item.Id == novoId)
            .ToDictionaryAsync(item => item.Id, item => item.Status);
    }

    private static Task<HttpResponseMessage> AceitarAsync(HttpClient cliente, Guid id, bool confirmarSubstituicao)
        => cliente.PostAsJsonAsync($"/api/v1/orcamentos/Aceitar/{id}", new { confirmarSubstituicao });

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        var cliente = factory.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new
        {
            email = "admin@integracao.test",
            senha = "Tenant@123"
        });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var token = (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
