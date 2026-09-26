using System.Net;
using System.Text;
using Officio.Infraestrutura.Integracoes;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class ConsultaCepViaCepTestes
{
    [Fact]
    public async Task ConsultarAsync_DeveMapearEnderecoECodigoIbge()
    {
        var handler = new RespostaHttpHandler(
            """
            {
              "cep": "89010-000",
              "logradouro": "Rua XV de Novembro",
              "complemento": "",
              "bairro": "Centro",
              "localidade": "Blumenau",
              "uf": "SC",
              "ibge": "4202404"
            }
            """);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://viacep.com.br/") };
        var consulta = new ConsultaCepViaCep(httpClient, TimeProvider.System);

        var endereco = await consulta.ConsultarAsync("89010000");

        Assert.NotNull(endereco);
        Assert.Equal("89010000", endereco.Cep);
        Assert.Equal("Rua XV de Novembro", endereco.Logradouro);
        Assert.Null(endereco.Complemento);
        Assert.Equal("Blumenau", endereco.Cidade);
        Assert.Equal("SC", endereco.Uf);
        Assert.Equal("4202404", endereco.CodigoMunicipioIbge);
        Assert.Equal("/ws/89010000/json/", handler.RequisicaoRecebida?.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task ConsultarAsync_DeveRetornarNuloParaCepInexistente()
    {
        var handler = new RespostaHttpHandler("""{ "erro": true }""");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://viacep.com.br/") };
        var consulta = new ConsultaCepViaCep(httpClient, TimeProvider.System);

        var endereco = await consulta.ConsultarAsync("99999998");

        Assert.Null(endereco);
    }

    [Fact]
    public async Task ConsultarAsync_DeveAceitarIndicadorDeErroComoTexto()
    {
        var handler = new RespostaHttpHandler("""{ "erro": "true" }""");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://viacep.com.br/") };
        var consulta = new ConsultaCepViaCep(httpClient, TimeProvider.System);

        var endereco = await consulta.ConsultarAsync("99999997");

        Assert.Null(endereco);
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
