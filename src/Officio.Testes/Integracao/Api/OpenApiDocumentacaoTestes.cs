using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class OpenApiDocumentacaoTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task DocumentoOpenApi_DescreveJwtEProtegeSomenteEndpointsAutorizados()
    {
        using var cliente = factory.CreateClient();
        var resposta = await cliente.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var documento = await resposta.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Officio API", documento!["info"]!["title"]!.GetValue<string>());

        var bearer = documento["components"]!["securitySchemes"]!["Bearer"]!;
        Assert.Equal("http", bearer["type"]!.GetValue<string>());
        Assert.Equal("bearer", bearer["scheme"]!.GetValue<string>());

        var login = documento["paths"]!["/api/v1/autenticacao/Login"]!["post"]!;
        Assert.Null(login["security"]);

        var listarProspectos = documento["paths"]!["/api/v1/prospectos/Listar"]!["get"]!;
        Assert.Equal("Prospectos_Listar", listarProspectos["operationId"]!.GetValue<string>());
        Assert.Equal("Bearer", listarProspectos["security"]![0]!.AsObject().Single().Key);
        Assert.NotNull(listarProspectos["responses"]!["401"]);
    }

    [Fact]
    public async Task PaginaSwagger_UsaAutorizacaoNativaPersistente()
    {
        using var cliente = factory.CreateClient();
        var resposta = await cliente.GetAsync("/swagger");
        var html = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("persistAuthorization: true", html);
        Assert.DoesNotContain("Definir JWT", html);
    }
}
