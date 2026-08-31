using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Integracoes.Autentique;
using Officio.Infraestrutura.Servicos;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class ServicoAutentiqueTestes
{
    [Fact]
    public async Task EnviarParaAssinaturaAsync_SemApiToken_RetornaFallbackSemChamarCliente()
    {
        var cliente = new AutentiqueIntegracaoClienteFake();
        var servico = CriarServico(cliente, new OpcoesAutentique { ApiToken = "" });

        var resultado = await servico.EnviarParaAssinaturaAsync(CriarSolicitacao());

        Assert.False(resultado.Sucesso);
        Assert.True(resultado.RequerFallback);
        Assert.Equal(0, cliente.QuantidadeChamadas);
    }

    [Fact]
    public async Task EnviarParaAssinaturaAsync_ComRetornoValido_RetornaResultadoAssinatura()
    {
        var cliente = new AutentiqueIntegracaoClienteFake
        {
            Resposta = new RespostaCriarDocumentoAutentique(
                new DadosCriarDocumentoAutentique(
                    new DocumentoAutentique(
                        "doc-1",
                        [
                            new AssinaturaAutentique("assinatura-1", new SignatarioAutentique("cliente@teste.com"))
                        ])))
        };
        var servico = CriarServico(cliente, new OpcoesAutentique { ApiToken = "token-teste" });

        var resultado = await servico.EnviarParaAssinaturaAsync(CriarSolicitacao());

        Assert.True(resultado.Sucesso);
        Assert.False(resultado.RequerFallback);
        Assert.Equal("doc-1", resultado.EnvelopeId);
        var link = Assert.Single(resultado.LinksAssinantes);
        Assert.Equal("cliente@teste.com", link.Email);
        Assert.Equal("https://app.autentique.com.br/assinatura/assinatura-1", link.Link);
        Assert.Equal(1, cliente.QuantidadeChamadas);
        Assert.Equal("token-teste", cliente.ApiTokenRecebido);
    }

    [Fact]
    public async Task EnviarParaAssinaturaAsync_ComFalhaEsperada_RetornaFallbackManual()
    {
        var cliente = new AutentiqueIntegracaoClienteFake
        {
            Excecao = new JsonException("json invalido")
        };
        var servico = CriarServico(cliente, new OpcoesAutentique { ApiToken = "token-teste" });

        var resultado = await servico.EnviarParaAssinaturaAsync(CriarSolicitacao());

        Assert.False(resultado.Sucesso);
        Assert.True(resultado.RequerFallback);
        Assert.Equal(1, cliente.QuantidadeChamadas);
    }

    private static ServicoAutentique CriarServico(
        IAutentiqueIntegracaoCliente cliente,
        OpcoesAutentique opcoes)
        => new(
            cliente,
            Options.Create(opcoes),
            NullLogger<ServicoAutentique>.Instance);

    private static SolicitacaoAssinatura CriarSolicitacao()
        => new(
            new MemoryStream(Encoding.UTF8.GetBytes("%PDF-1.4 teste")),
            "contrato.pdf",
            [new DadosAssinante("Cliente Teste", "cliente@teste.com", "12345678901", "Cliente")]);

    private sealed class AutentiqueIntegracaoClienteFake : IAutentiqueIntegracaoCliente
    {
        public int QuantidadeChamadas { get; private set; }
        public string? ApiTokenRecebido { get; private set; }
        public RespostaCriarDocumentoAutentique? Resposta { get; init; }
        public Exception? Excecao { get; init; }

        public Task<RespostaCriarDocumentoAutentique?> CriarDocumentoAsync(
            SolicitacaoAssinatura request,
            string apiToken,
            CancellationToken cancellationToken = default)
        {
            QuantidadeChamadas++;
            ApiTokenRecebido = apiToken;

            if (Excecao is not null)
                throw Excecao;

            return Task.FromResult(Resposta);
        }
    }
}
