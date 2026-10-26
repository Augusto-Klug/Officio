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
                        "Contrato Teste",
                        [
                            new AssinaturaAutentique("assinatura-1", "cliente@teste.com", new LinkAutentique("https://link.test/assinar-cliente")),
                            new AssinaturaAutentique("assinatura-2", "contador@teste.com", new LinkAutentique("https://link.test/assinar-contador"))
                        ])),
                null)
        };
        var servico = CriarServico(cliente, new OpcoesAutentique { ApiToken = "token-teste" });

        var resultado = await servico.EnviarParaAssinaturaAsync(CriarSolicitacao());

        Assert.True(resultado.Sucesso);
        Assert.False(resultado.RequerFallback);
        Assert.Equal("doc-1", resultado.EnvelopeId);
        Assert.Collection(resultado.LinksAssinantes,
            link =>
            {
                Assert.Equal("cliente@teste.com", link.Email);
                Assert.Equal("https://link.test/assinar-cliente", link.Link);
                Assert.Equal("Cliente Teste", link.Nome);
                Assert.Equal("contratante", link.Tipo);
            },
            link =>
            {
                Assert.Equal("contador@teste.com", link.Email);
                Assert.Equal("https://link.test/assinar-contador", link.Link);
                Assert.Equal("Contador Teste", link.Nome);
                Assert.Equal("contratada", link.Tipo);
            });
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

    [Fact]
    public async Task BaixarDocumentoAssinadoAsync_ComUrlAssinada_BaixaPdf()
    {
        var cliente = new AutentiqueIntegracaoClienteFake
        {
            RespostaDocumento = new RespostaObterDocumentoAutentique(
                new DadosObterDocumentoAutentique(
                    new DocumentoArquivoAutentique(
                        "doc-1",
                        "Contrato Teste",
                        new ArquivosAutentique(null, "https://arquivos.test/assinado.pdf", null, null))),
                null),
            ConteudoArquivo = new MemoryStream([1, 2, 3])
        };
        var servico = CriarServico(cliente, new OpcoesAutentique { ApiToken = "token-teste" });

        var resultado = await servico.BaixarDocumentoAssinadoAsync("doc-1");

        Assert.True(resultado.Sucesso, resultado.Mensagem);
        Assert.Equal("Contrato Teste-assinado.pdf", resultado.NomeArquivo);
        Assert.Equal([1, 2, 3], await LerBytesAsync(resultado.Conteudo!));
        Assert.Equal("doc-1", cliente.DocumentoIdRecebido);
        Assert.Equal("https://arquivos.test/assinado.pdf", cliente.UrlBaixada);
    }

    [Fact]
    public async Task BaixarDocumentoAssinadoAsync_SemUrlAssinada_RetornaFalha()
    {
        var cliente = new AutentiqueIntegracaoClienteFake
        {
            RespostaDocumento = new RespostaObterDocumentoAutentique(
                new DadosObterDocumentoAutentique(
                    new DocumentoArquivoAutentique("doc-1", "Contrato Teste", new ArquivosAutentique(null, null, null, null))),
                null)
        };
        var servico = CriarServico(cliente, new OpcoesAutentique { ApiToken = "token-teste" });

        var resultado = await servico.BaixarDocumentoAssinadoAsync("doc-1");

        Assert.False(resultado.Sucesso);
        Assert.Contains("URL", resultado.Mensagem, StringComparison.OrdinalIgnoreCase);
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
            [
                new DadosAssinante("Cliente Teste", "cliente@teste.com", "12345678901", "contratante"),
                new DadosAssinante("Contador Teste", "contador@teste.com", "98765432100", "contratada")
            ]);

    private static async Task<byte[]> LerBytesAsync(Stream stream)
    {
        await using var conteudo = stream;
        using var memoria = new MemoryStream();
        await conteudo.CopyToAsync(memoria);
        return memoria.ToArray();
    }

    private sealed class AutentiqueIntegracaoClienteFake : IAutentiqueIntegracaoCliente
    {
        public int QuantidadeChamadas { get; private set; }
        public string? ApiTokenRecebido { get; private set; }
        public string? DocumentoIdRecebido { get; private set; }
        public string? UrlBaixada { get; private set; }
        public RespostaCriarDocumentoAutentique? Resposta { get; init; }
        public RespostaObterDocumentoAutentique? RespostaDocumento { get; init; }
        public Stream ConteudoArquivo { get; init; } = new MemoryStream();
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

        public Task<RespostaObterDocumentoAutentique?> ObterDocumentoAsync(
            string documentoId,
            string apiToken,
            CancellationToken cancellationToken = default)
        {
            DocumentoIdRecebido = documentoId;
            ApiTokenRecebido = apiToken;

            if (Excecao is not null)
                throw Excecao;

            return Task.FromResult(RespostaDocumento);
        }

        public Task<Stream> BaixarArquivoAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            UrlBaixada = url;
            return Task.FromResult(ConteudoArquivo);
        }
    }
}
