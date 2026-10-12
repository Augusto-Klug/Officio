using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class HealthCheckTestes
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthCheck_RetornaSucesso(string rota)
    {
        using var factory = new OfficioWebApplicationFactory();
        await factory.ReiniciarBancoAsync();
        using var cliente = factory.CreateClient();

        using var resposta = await cliente.GetAsync(rota);

        Assert.Equal(System.Net.HttpStatusCode.OK, resposta.StatusCode);
    }
}
