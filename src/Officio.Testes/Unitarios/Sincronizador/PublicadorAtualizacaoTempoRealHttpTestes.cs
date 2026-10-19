extern alias Sincronizador;

using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Officio.Dominio.Eventos;
using PublicadorAtualizacaoTempoRealHttp = Sincronizador::Officio.Sincronizador.TempoReal.PublicadorAtualizacaoTempoRealHttp;

namespace Officio.Testes.Unitarios.Sincronizador;

public sealed class PublicadorAtualizacaoTempoRealHttpTestes
{
    [Fact]
    public async Task PublicarAsync_EnviaUrlHeaderEEvento()
    {
        var handler = new HandlerFake(HttpStatusCode.NoContent);
        using var clienteHttp = new HttpClient(handler);
        var publicador = CriarPublicador(clienteHttp);
        var evento = new EventoAtualizacaoTempoReal(
            Guid.NewGuid(), EntidadesAtualizacaoTempoReal.Contrato, Guid.NewGuid());

        await publicador.PublicarAsync(evento);

        Assert.Equal("http://127.0.0.1:8080/api/v1/interno/atualizacoes-tempo-real", handler.Url?.ToString());
        Assert.Equal("segredo-compartilhado-teste", handler.ChaveRecebida);
        Assert.Equal(
            evento,
            JsonSerializer.Deserialize<EventoAtualizacaoTempoReal>(handler.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task PublicarAsync_RespostaNaoSucesso_NaoPropagaFalha(HttpStatusCode status)
    {
        using var clienteHttp = new HttpClient(new HandlerFake(status));
        var publicador = CriarPublicador(clienteHttp);

        var exception = await Record.ExceptionAsync(() => publicador.PublicarAsync(
            new EventoAtualizacaoTempoReal(Guid.NewGuid(), EntidadesAtualizacaoTempoReal.Contrato)));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PublicarAsync_FalhaDeRede_NaoPropagaFalha()
    {
        using var clienteHttp = new HttpClient(new HandlerFake(new HttpRequestException("rede indisponivel")));
        var publicador = CriarPublicador(clienteHttp);

        var exception = await Record.ExceptionAsync(() => publicador.PublicarAsync(
            new EventoAtualizacaoTempoReal(Guid.NewGuid(), EntidadesAtualizacaoTempoReal.Contrato)));

        Assert.Null(exception);
    }

    private static PublicadorAtualizacaoTempoRealHttp CriarPublicador(HttpClient cliente)
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TempoReal:CallbackUrl"] = "http://127.0.0.1:8080",
                ["TempoReal:ChaveCompartilhada"] = "segredo-compartilhado-teste"
            })
            .Build();
        return new PublicadorAtualizacaoTempoRealHttp(
            cliente, configuracao, NullLogger<PublicadorAtualizacaoTempoRealHttp>.Instance);
    }

    private sealed class HandlerFake(HttpStatusCode status) : HttpMessageHandler
    {
        private readonly Exception? falha = null;

        public HandlerFake(Exception exception) : this(HttpStatusCode.OK) => falha = exception;

        public Uri? Url { get; private set; }
        public string? ChaveRecebida { get; private set; }
        public string Body { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (falha is not null)
                throw falha;

            Url = request.RequestUri;
            ChaveRecebida = request.Headers.GetValues("X-Officio-Internal-Key").Single();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status);
        }
    }
}
