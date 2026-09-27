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

        var consultarCep = documento["paths"]!["/api/v1/localidades/cep/{cep}"]!["get"]!;
        Assert.Contains("200", consultarCep["responses"]!.AsObject().Select(response => response.Key));
        Assert.Contains("400", consultarCep["responses"]!.AsObject().Select(response => response.Key));
        Assert.Contains("404", consultarCep["responses"]!.AsObject().Select(response => response.Key));
        Assert.Contains("503", consultarCep["responses"]!.AsObject().Select(response => response.Key));
        Assert.Contains("401", consultarCep["responses"]!.AsObject().Select(response => response.Key));

        var criarProspecto = documento["paths"]!["/api/v1/prospectos/Criar"]!["post"]!;
        foreach (var status in new[] { "201", "400", "422", "503", "401" })
            Assert.Contains(status, criarProspecto["responses"]!.AsObject().Select(response => response.Key));

        var webhook = documento["paths"]!["/api/v1/contratos/webhook/autentique"]!["post"]!;
        foreach (var status in new[] { "202", "401", "503" })
            Assert.Contains(status, webhook["responses"]!.AsObject().Select(response => response.Key));

        foreach (var (path, item) in documento["paths"]!.AsObject())
        foreach (var (method, operationNode) in item!.AsObject())
        {
            if (method is not ("get" or "post" or "put" or "patch" or "delete"))
                continue;

            var operation = operationNode!.AsObject();
            var responses = operation["responses"]!.AsObject();
            var statusCodes = responses.Select(response => response.Key).ToArray();
            Assert.True(statusCodes.Any(status => status.StartsWith('2')),
                $"{method.ToUpperInvariant()} {path} não documenta resposta de sucesso.");

            Assert.All(statusCodes, status =>
                Assert.Matches("^(1|2|3|4|5)[0-9]{2}$", status));

            if (operation.ContainsKey("security"))
                Assert.Contains("401", statusCodes);
        }
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
