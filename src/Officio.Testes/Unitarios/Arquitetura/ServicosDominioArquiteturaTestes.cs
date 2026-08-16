using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Servicos;

namespace Officio.Testes.Unitarios.Arquitetura;

public class ServicosDominioArquiteturaTestes
{
    [Theory]
    [InlineData("Officio.Dominio.Servicos.Servicos.ProspectoServico")]
    [InlineData("Officio.Dominio.Servicos.Servicos.DocumentoServico")]
    [InlineData("Officio.Dominio.Servicos.Servicos.ContratoServico")]
    public void ServicosDominio_NaoDevemReceberDbContextNoConstrutor(string nomeTipoServico)
    {
        var tipoServico = typeof(InstalarDependencias).Assembly.GetType(nomeTipoServico);
        Assert.NotNull(tipoServico);

        var construtores = tipoServico.GetConstructors(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        Assert.DoesNotContain(
            construtores.SelectMany(c => c.GetParameters()),
            parametro => typeof(DbContext).IsAssignableFrom(parametro.ParameterType));
    }
}
