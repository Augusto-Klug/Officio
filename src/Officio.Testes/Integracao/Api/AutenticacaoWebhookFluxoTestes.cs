using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class AutenticacaoWebhookFluxoTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task Login_ComSenhaIncorretaRetorna401_EApiProtegidaSemTokenRetorna401()
    {
        await factory.PrepararTenantAsync();
        using var cliente = factory.CreateClient();

        var login = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new
        {
            email = "admin@integracao.test",
            senha = "senha-incorreta"
        });
        var acessoProtegido = await cliente.GetAsync("/api/v1/prospectos/Listar");

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, acessoProtegido.StatusCode);
    }

    [Fact]
    public async Task WebhookAutentique_ComHmacValidoAceitaEEnfileiraPayload()
    {
        await factory.ReiniciarBancoAsync();
        using var cliente = factory.CreateClient();
        const string payload = "{\"event\":\"document.finished\",\"data\":{\"id\":\"doc-integracao-1\"}}";
        using var requisicao = CriarRequisicaoWebhook(payload, "segredo-webhook-integracao");

        var resposta = await cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Accepted, resposta.StatusCode);
        Assert.Equal(payload, Assert.Single(factory.FilaWebhook.Payloads));
    }

    [Fact]
    public async Task WebhookAutentique_ComHmacInvalidoRetorna401ENaoEnfileira()
    {
        await factory.ReiniciarBancoAsync();
        using var cliente = factory.CreateClient();
        using var requisicao = CriarRequisicaoWebhook("{\"event\":\"document.finished\"}", "segredo-errado");

        var resposta = await cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Empty(factory.FilaWebhook.Payloads);
    }

    [Fact]
    public async Task WebhookAutentique_QuandoFilaIndisponivelRetorna503()
    {
        await factory.ReiniciarBancoAsync();
        factory.FilaWebhook.AceitarEnfileiramento = false;
        using var cliente = factory.CreateClient();
        using var requisicao = CriarRequisicaoWebhook("{}", "segredo-webhook-integracao");

        var resposta = await cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        Assert.Empty(factory.FilaWebhook.Payloads);
    }

    private static HttpRequestMessage CriarRequisicaoWebhook(string payload, string segredo)
    {
        var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/v1/contratos/webhook/autentique")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(segredo), Encoding.UTF8.GetBytes(payload));
        requisicao.Headers.Add("x-autentique-signature", Convert.ToHexString(hash).ToLowerInvariant());
        return requisicao;
    }
}
