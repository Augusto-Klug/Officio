using Microsoft.AspNetCore.Hosting;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class CorsFrontendTestes
{
    [Theory]
    [InlineData("https://piloto.azurestaticapps.net", true)]
    [InlineData("https://outro.example", false)]
    public async Task Preflight_AceitaSomenteOrigemConfigurada(string origem, bool permitida)
    {
        using var factory = new OfficioWebApplicationFactory();
        using var app = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Cors:AllowedOrigins:0", "https://piloto.azurestaticapps.net"));
        using var cliente = app.CreateClient();
        using var requisicao = new HttpRequestMessage(HttpMethod.Options, "/api/v1/autenticacao/Login");
        requisicao.Headers.Add("Origin", origem);
        requisicao.Headers.Add("Access-Control-Request-Method", "POST");
        requisicao.Headers.Add("Access-Control-Request-Headers", "authorization");

        using var resposta = await cliente.SendAsync(requisicao);
        var origemPermitida = resposta.Headers.TryGetValues("Access-Control-Allow-Origin", out var valores)
            ? Assert.Single(valores)
            : null;

        Assert.Equal(permitida ? origem : null, origemPermitida);
    }
}
