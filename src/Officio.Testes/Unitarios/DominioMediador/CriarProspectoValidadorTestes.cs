using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

namespace Officio.Testes.Unitarios.DominioMediador;

public class CriarProspectoValidadorTestes
{
    [Theory]
    [InlineData("99093538000155")]
    [InlineData("99.093.538/0001-55")]
    public void Validar_AceitaCnpjComOuSemPontuacao(string cnpj)
    {
        var comando = new CriarProspectoComando(CriarRequisicao(cnpj: cnpj));

        var resultado = new CriarProspectoValidador().Validate(comando);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Validar_RejeitaCnpjComQuantidadeIncorretaDeDigitos()
    {
        var comando = new CriarProspectoComando(CriarRequisicao(cnpj: "9909353800015"));

        var resultado = new CriarProspectoValidador().Validate(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.ErrorMessage == "CNPJ inválido.");
    }
    [Fact]
    public void Validar_RejeitaProspectoSemNumeroDeEndereco()
    {
        var comando = new CriarProspectoComando(CriarRequisicao(numero: null));

        var resultado = new CriarProspectoValidador().Validate(comando);

        Assert.Contains(resultado.Errors, erro => erro.PropertyName == "Requisicao.Numero");
    }

    [Fact]
    public void Validar_RejeitaEnderecoDeSocioSemNumero()
    {
        var socio = new CriarSocioRequisicao(Endereco: new EnderecoSocioRequisicao(Logradouro: "Rua das Flores"));
        var comando = new CriarProspectoComando(CriarRequisicao(socios: [socio]));

        var resultado = new CriarProspectoValidador().Validate(comando);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Validar_ExigeContatoTelefoneEEnderecoDoProspecto()
    {
        var comando = new CriarProspectoComando(CriarRequisicao(
            nomeContato: null, email: null, telefone: null, cep: null, logradouro: null,
            bairro: null, uf: null, cidade: null, municipioCodigoIbge: null));

        var resultado = new CriarProspectoValidador().Validate(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("NomeContato"));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("Email"));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("Telefone"));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("Cep"));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("Logradouro"));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("Bairro"));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("Uf"));
        Assert.Contains(resultado.Errors, erro => erro.PropertyName.EndsWith("MunicipioCodigoIbge"));
    }

    private static CriarProspectoRequisicao CriarRequisicao(
        string? cnpj = null,
        IReadOnlyCollection<CriarSocioRequisicao>? socios = null,
        string? numero = "10",
        string? nomeContato = "Contato",
        string? email = "contato@example.com",
        string? telefone = "4733334444",
        string? cep = "89010000",
        string? logradouro = "Rua das Flores",
        string? bairro = "Centro",
        string? uf = "SC",
        string? cidade = "Blumenau",
        string? municipioCodigoIbge = "4202404") =>
        new("Teste Augusto", cnpj, null, socios,
            NomeContato: nomeContato,
            Email: email,
            Telefone: telefone,
            Cep: cep,
            Logradouro: logradouro,
            Numero: numero,
            Bairro: bairro,
            Uf: uf,
            Cidade: cidade,
            MunicipioCodigoIbge: municipioCodigoIbge);
}
