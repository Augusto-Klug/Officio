using Officio.Dominio.Enumeradores;
using Officio.Infraestrutura.Servicos;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class NormalizadorCamposOcrTestes
{
    [Theory]
    [InlineData("Driver License", "CNH")]
    [InlineData("ID Card", "IDENTIDADE")]
    public void Normalizar_ClassificaDocumentosDeIdentificacaoPeloTipoAzure(string tipoAzure, string esperado)
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.Identidade,
        [
            new CampoOcrBruto("DocumentType", tipoAzure, 0.99m),
            new CampoOcrBruto("FirstName", "Ana", 0.98m),
            new CampoOcrBruto("LastName", "Silva", 0.98m)
        ]);

        Assert.Equal(esperado, resultado.Valores["tipo_documento_detectado"]);
    }

    [Fact]
    public void Normalizar_ClassificaContaComEnderecoComoComprovanteResidencia()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.ComprovanteResidencia,
        [
            new CampoOcrBruto("Address", "Rua das Flores, 123", 0.95m),
            new CampoOcrBruto("__linha_0_0", "FATURA DE ENERGIA ELETRICA", 0.95m),
            new CampoOcrBruto("__linha_0_1", "CEP 89075-255", 0.94m)
        ]);

        Assert.Equal("COMPROVANTE_RESIDENCIA", resultado.Valores["tipo_documento_detectado"]);
    }

    [Fact]
    public void Normalizar_SinaisDeFaturaSobrepoemTipoAzureDeCnh()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.Cnh,
        [
            new CampoOcrBruto("DocumentType", "Driver License", 0.97m),
            new CampoOcrBruto("Address", "Rua das Flores, 123", 0.95m),
            new CampoOcrBruto("__linha_0_0", "FATURA DE ENERGIA ELETRICA", 0.95m),
            new CampoOcrBruto("__linha_0_1", "CEP 89075-255", 0.94m)
        ]);

        Assert.Equal("COMPROVANTE_RESIDENCIA", resultado.Valores["tipo_documento_detectado"]);
    }

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

    [Fact]
    public void Normalizar_CinUsaNumeroDeOnzeDigitosComoCpf()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.Identidade,
            [new CampoOcrBruto("DocumentNumber", "117.892.999-00", 0.96m)]);

        Assert.Equal("11789299900", resultado.Valores["cpf"]);
        Assert.DoesNotContain("rg", resultado.Valores.Keys);
    }

    [Fact]
    public void Normalizar_IdentidadeExtraiNomeDoTextoLivreSemInferirCamposAusentes()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.Identidade,
        [
            new CampoOcrBruto("__linha_0_0", "REPUBLICA FEDERATIVA DO BRASIL", 0.95m),
            new CampoOcrBruto("__linha_0_1", "Maria Oliveira Santos", 0.93m),
            new CampoOcrBruto("__linha_0_2", "ASSINATURA DO TITULAR", 0.94m)
        ]);

        Assert.Equal("Maria Oliveira Santos", resultado.Valores["nome"]);
        Assert.Equal(0.93m, resultado.Confiancas["nome"]);
        Assert.DoesNotContain("cpf", resultado.Valores.Keys);
        Assert.DoesNotContain("rg", resultado.Valores.Keys);
        Assert.DoesNotContain("data_nascimento", resultado.Valores.Keys);
    }

    [Theory]
    [InlineData(TipoDocumento.Cpf, "117.892.999-00", "cpf", "11789299900")]
    [InlineData(TipoDocumento.CartaoCnpj, "11.222.333/0001-44", "cnpj", "11222333000144")]
    public void Normalizar_TextoLivreExtraiDocumentoFiscal(
        TipoDocumento tipo,
        string texto,
        string campo,
        string esperado)
    {
        var resultado = NormalizadorCamposOcr.Normalizar(tipo,
            [new CampoOcrBruto("__linha_0_0", texto, 0.91m)]);

        Assert.Equal(esperado, resultado.Valores[campo]);
        Assert.Equal(0.91m, resultado.Confiancas[campo]);
    }

    [Theory]
    [InlineData(TipoDocumento.Cnh)]
    [InlineData(TipoDocumento.TituloEleitor)]
    [InlineData(TipoDocumento.CarteiraTrabalho)]
    [InlineData(TipoDocumento.CertificadoReservista)]
    public void Normalizar_DocumentoPessoalExtraiNomeECpfDoTextoLivre(TipoDocumento tipo)
    {
        var resultado = NormalizadorCamposOcr.Normalizar(tipo,
        [
            new CampoOcrBruto("__linha_0_0", "NOME", 0.96m),
            new CampoOcrBruto("__linha_0_1", "Marina Gabriela Fusina", 0.94m),
            new CampoOcrBruto("__linha_0_2", "CPF 117.892.999-00", 0.92m)
        ]);

        Assert.Equal("Marina Gabriela Fusina", resultado.Valores["nome"]);
        Assert.Equal("11789299900", resultado.Valores["cpf"]);
    }

    [Fact]
    public void Normalizar_CarteiraTrabalhoDigitalExtraiNomeCivilECpf()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.CarteiraTrabalho,
        [
            new CampoOcrBruto("__linha_0_0", "Dados Pessoais", 0.99m),
            new CampoOcrBruto("__linha_0_1", "Nome civil", 0.99m),
            new CampoOcrBruto("__linha_0_2", "AUGUSTO KLUG DE ABREU", 0.99m),
            new CampoOcrBruto("__linha_0_3", "CPF", 0.99m),
            new CampoOcrBruto("__linha_0_4", "112.020.749-52", 0.99m),
            new CampoOcrBruto("__linha_0_5", "Data de nascimento", 0.99m),
            new CampoOcrBruto("__linha_0_6", "24/08/2004", 0.99m)
        ]);

        Assert.Equal("AUGUSTO KLUG DE ABREU", resultado.Valores["nome"]);
        Assert.Equal("11202074952", resultado.Valores["cpf"]);
    }

    [Fact]
    public void Normalizar_TituloEleitorExtraiNumeroRotulado()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.TituloEleitor,
        [
            new CampoOcrBruto("__linha_0_0", "Número do título", 0.95m),
            new CampoOcrBruto("__linha_0_1", "1234 5678 9012", 0.93m)
        ]);

        Assert.Equal("1234 5678 9012", resultado.Valores["numero_titulo"]);
    }

    [Fact]
    public void Normalizar_TituloEleitoralDigitalDoTseExtraiNomeInscricaoENascimentoSemCpf()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.TituloEleitor,
        [
            new CampoOcrBruto("__linha_0_0", "TÍTULO ELEITORAL", 0.99m),
            new CampoOcrBruto("__linha_0_1", "NOME DO ELEITOR", 0.99m),
            new CampoOcrBruto("__linha_0_2", "ANA MARIA DOS SANTOS", 0.99m),
            new CampoOcrBruto("__linha_0_3", "DATA DE NASCIMENTO", 0.99m),
            new CampoOcrBruto("__linha_0_4", "24/08/2004", 0.99m),
            new CampoOcrBruto("__linha_0_5", "INSCRIÇÃO", 0.99m),
            new CampoOcrBruto("__linha_0_6", "067278480957", 0.99m),
            new CampoOcrBruto("Data de nascimento", "24/08/2004", 0.99m),
            new CampoOcrBruto("tituloeleitoral", "NOME DO ELEITOR", 0.97m)
        ]);

        Assert.Equal("ANA MARIA DOS SANTOS", resultado.Valores["nome"]);
        Assert.Equal("067278480957", resultado.Valores["numero_titulo"]);
        Assert.Equal("2004-08-24", resultado.Valores["data_nascimento"]);
        Assert.DoesNotContain("cpf", resultado.Valores.Keys);
    }

    [Fact]
    public void Normalizar_ComprovanteResidenciaExtraiNomeECep()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.ComprovanteResidencia,
        [
            new CampoOcrBruto("__linha_0_0", "Nome do titular", 0.96m),
            new CampoOcrBruto("__linha_0_1", "Marina Gabriela Fusina", 0.94m),
            new CampoOcrBruto("__linha_0_2", "CEP 89075-255", 0.92m)
        ]);

        Assert.Equal("Marina Gabriela Fusina", resultado.Valores["nome"]);
        Assert.Equal("89075-255", resultado.Valores["cep"]);
    }

    [Fact]
    public void Normalizar_ComprovanteEscolaridadeExtraiCamposRotulados()
    {
        var resultado = NormalizadorCamposOcr.Normalizar(TipoDocumento.ComprovanteEscolaridade,
        [
            new CampoOcrBruto("__linha_0_0", "Nome do aluno", 0.96m),
            new CampoOcrBruto("__linha_0_1", "Marina Gabriela Fusina", 0.94m),
            new CampoOcrBruto("__linha_0_2", "Instituição de ensino", 0.93m),
            new CampoOcrBruto("__linha_0_3", "Escola Municipal", 0.92m),
            new CampoOcrBruto("__linha_0_4", "Curso: Ensino Médio", 0.91m)
        ]);

        Assert.Equal("Marina Gabriela Fusina", resultado.Valores["nome"]);
        Assert.Equal("Escola Municipal", resultado.Valores["instituicao"]);
        Assert.Equal("Ensino Médio", resultado.Valores["curso"]);
    }
}
