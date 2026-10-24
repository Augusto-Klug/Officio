using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Officio.Infraestrutura.Integracoes;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class ConsultaMunicipioIbgeTestes
{
    [Fact]
    public async Task ObterPorNomeUfAsync_DeveConverterIdNumericoDoIbgeParaCodigo()
    {
        var handler = new RespostaHttpHandler(
            """
            [
              { "id": 4202404, "nome": "Blumenau" },
              { "id": 4205407, "nome": "Florianopolis" }
            ]
            """);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://servicodados.ibge.gov.br/")
        };
        var consulta = new ConsultaMunicipioIbge(httpClient, NullLogger<ConsultaMunicipioIbge>.Instance);

        var municipio = await consulta.ObterPorNomeUfAsync("blumenau", "sc");

        Assert.NotNull(municipio);
        Assert.Equal("4202404", municipio.CodigoIbge);
        Assert.Equal("Blumenau", municipio.Nome);
        Assert.Equal("SC", municipio.Uf);
        Assert.Equal(
            "/api/v1/localidades/estados/SC/municipios",
            handler.RequisicaoRecebida?.RequestUri?.AbsolutePath);
    }

    private sealed class RespostaHttpHandler(string resposta) : HttpMessageHandler
    {
        public HttpRequestMessage? RequisicaoRecebida { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequisicaoRecebida = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(resposta, Encoding.UTF8, "application/json")
            });
        }
    }
}
