using Officio.Dominio.Enumeradores;
using Officio.Infraestrutura.Servicos;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class NormalizadorCamposOcrTestes
{
    [Fact]
    public void Normalizar_IdentidadeMapeiaCamposDoModeloPreConstruido()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.Identidade,
        [
            new CampoOcrBruto("FirstName", "Ana", 0.98m),
            new CampoOcrBruto("LastName", "Silva", 0.97m),
            new CampoOcrBruto("PersonalNumber", "117.892.999-00", 0.96m),
            new CampoOcrBruto("DocumentNumber", "12.345.678-9", 0.95m),
            new CampoOcrBruto("DateOfBirth", "10/05/1995", 0.94m)
        ]);

        Assert.Equal("Ana Silva", resultado.Valores["nome"]);
        Assert.Equal("11789299900", resultado.Valores["cpf"]);
        Assert.Equal("12.345.678-9", resultado.Valores["rg"]);
        Assert.Equal("1995-05-10", resultado.Valores["data_nascimento"]);
        Assert.Equal(0.97m, resultado.Confiancas["nome"]);
    }

    [Fact]
    public void Normalizar_DocumentoGenericoMapeiaRotulosEmPortugues()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.CartaoCnpj,
        [
            new CampoOcrBruto("Razão Social", "Empresa Exemplo Ltda", 0.91m),
            new CampoOcrBruto("CNPJ:", "11.222.333/0001-44", 0.93m),
            new CampoOcrBruto("Nome Fantasia", "Empresa Exemplo", 0.89m)
        ]);

        Assert.Equal("Empresa Exemplo Ltda", resultado.Valores["razao_social"]);
        Assert.Equal("11222333000144", resultado.Valores["cnpj"]);
        Assert.Equal("Empresa Exemplo", resultado.Valores["nome_fantasia"]);
    }

    [Theory]
    [InlineData(TipoDocumento.Cpf, "cpf")]
    [InlineData(TipoDocumento.CpfSocio, "cpf")]
    [InlineData(TipoDocumento.Identidade, "rg")]
    [InlineData(TipoDocumento.RgSocio, "rg")]
    public void Normalizar_NumeroDocumentoRespeitaTipo(TipoDocumento tipo, string campoEsperado)
    {
        var resultado = NormalizadorCamposOcr.Normalizar(tipo,
            [new CampoOcrBruto("DocumentNumber", "123456789", 0.88m)]);

        Assert.Equal("123456789", resultado.Valores[campoEsperado]);
    }
}
