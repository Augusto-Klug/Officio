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
}
