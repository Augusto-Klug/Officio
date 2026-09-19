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
        var comando = new CriarProspectoComando(new(
            "Teste Augusto",
            cnpj,
            null,
            []));

        var resultado = new CriarProspectoValidador().Validate(comando);

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Validar_RejeitaCnpjComQuantidadeIncorretaDeDigitos()
    {
        var comando = new CriarProspectoComando(new(
            "Teste Augusto",
            "9909353800015",
            null,
            []));

        var resultado = new CriarProspectoValidador().Validate(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, erro => erro.ErrorMessage == "CNPJ inválido.");
    }
}
