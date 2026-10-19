using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public sealed class PublicacaoAtualizacaoInternaTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task Post_SemChaveValida_Retorna401ENaoPublica()
    {
        await factory.PrepararTenantAsync();
        var publicador = new PublicadorAtualizacaoFake();
        using var app = CriarApi(publicador);
        using var cliente = app.CreateClient();

        using var resposta = await cliente.PostAsJsonAsync(
            Rota,
            new EventoAtualizacaoTempoReal(Guid.NewGuid(), EntidadesAtualizacaoTempoReal.Contrato, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Empty(publicador.Eventos);
    }

    [Theory]
    [InlineData(EntidadesAtualizacaoTempoReal.Contrato)]
    [InlineData(EntidadesAtualizacaoTempoReal.CatalogoOrcamento)]
    public async Task Post_ComChaveValida_PublicaEventoE204(string entidade)
    {
        await factory.PrepararTenantAsync();
        var publicador = new PublicadorAtualizacaoFake();
        using var app = CriarApi(publicador);
        using var cliente = app.CreateClient();
        var evento = new EventoAtualizacaoTempoReal(Guid.NewGuid(), entidade, Guid.NewGuid());
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, Rota)
        {
            Content = JsonContent.Create(evento)
        };
        requisicao.Headers.Add(HeaderChave, Chave);

        using var resposta = await cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(evento, Assert.Single(publicador.Eventos));
    }

    [Fact]
    public async Task Post_EntidadeForaDaAllowlist_Retorna400ENaoPublica()
    {
        await factory.PrepararTenantAsync();
        var publicador = new PublicadorAtualizacaoFake();
        using var app = CriarApi(publicador);
        using var cliente = app.CreateClient();
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, Rota)
        {
            Content = JsonContent.Create(new EventoAtualizacaoTempoReal(Guid.NewGuid(), "entidade-arbitraria"))
        };
        requisicao.Headers.Add(HeaderChave, Chave);

        using var resposta = await cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Empty(publicador.Eventos);
    }

    private const string Rota = "/api/v1/interno/atualizacoes-tempo-real";
    private const string HeaderChave = "X-Officio-Internal-Key";
    private const string Chave = "chave-interna-de-integracao-para-testes";

    private WebApplicationFactory<Program> CriarApi(PublicadorAtualizacaoFake publicador)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TempoReal:ChaveCompartilhada"] = Chave
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPublicadorAtualizacaoTempoReal>();
                services.AddSingleton<IPublicadorAtualizacaoTempoReal>(publicador);
            });
        });
    }

    private sealed class PublicadorAtualizacaoFake : IPublicadorAtualizacaoTempoReal
    {
        public List<EventoAtualizacaoTempoReal> Eventos { get; } = [];

        public Task PublicarAsync(
            EventoAtualizacaoTempoReal evento,
            CancellationToken cancellationToken = default)
        {
            Eventos.Add(evento);
            return Task.CompletedTask;
        }
    }
}
