using Azure;
using Azure.AI.DocumentIntelligence;
using Officio.Dominio.Enumeradores;
using Officio.Infraestrutura.Servicos;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class ServicoOcrAzureTestes
{
    [Theory]
    [InlineData(TipoDocumento.CartaoCnpj)]
    [InlineData(TipoDocumento.ContratoSocial)]
    [InlineData(TipoDocumento.Cpf)]
    [InlineData(TipoDocumento.CarteiraTrabalho)]
    public void CriarOpcoes_DocumentoGenericoUsaLayoutComCamposChaveValorEmPortugues(TipoDocumento tipo)
    {
        var opcoes = ServicoOcrAzure.CriarOpcoes(tipo, BinaryData.FromBytes([1]));

        Assert.Equal("prebuilt-layout", opcoes.ModelId);
        Assert.Equal("pt-BR", opcoes.Locale);
        Assert.Contains(DocumentAnalysisFeature.KeyValuePairs, opcoes.Features);
    }

    [Theory]
    [InlineData(TipoDocumento.Identidade)]
    [InlineData(TipoDocumento.RgSocio)]
    [InlineData(TipoDocumento.Cnh)]
    [InlineData(TipoDocumento.CnhSocio)]
    public void CriarOpcoes_DocumentoOficialUsaModeloDeIdentidade(TipoDocumento tipo)
    {
        var opcoes = ServicoOcrAzure.CriarOpcoes(tipo, BinaryData.FromBytes([1]));

        Assert.Equal("prebuilt-idDocument", opcoes.ModelId);
        Assert.Null(opcoes.Locale);
        Assert.Empty(opcoes.Features);
    }

    [Theory]
    [InlineData(401, "CREDENCIAIS_INVALIDAS")]
    [InlineData(403, "CREDENCIAIS_INVALIDAS")]
    [InlineData(408, "TIMEOUT")]
    [InlineData(413, "DOCUMENTO_REJEITADO")]
    [InlineData(429, "LIMITE_REQUISICOES")]
    [InlineData(503, "AZURE_INDISPONIVEL")]
    [InlineData(400, "REQUISICAO_REJEITADA")]
    [InlineData(0, "FALHA_REDE")]
    public void ClassificarFalha_ConverteStatusHttpEmDiagnostico(int status, string categoria)
        => Assert.Equal(categoria, ServicoOcrAzure.ClassificarFalha(status));

    [Fact]
    public void Desembrulhar_AggregateExceptionPreservaFalhaHttpDoAzure()
    {
        var falhaAzure = new RequestFailedException(429, "Rate limit", "TooManyRequests", null);
        var aggregate = new AggregateException(new InvalidOperationException(), falhaAzure);

        Assert.Same(falhaAzure, ServicoOcrAzure.Desembrulhar(aggregate));
    }
}
