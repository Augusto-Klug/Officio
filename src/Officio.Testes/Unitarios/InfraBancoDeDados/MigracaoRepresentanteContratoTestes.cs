using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Officio.Infra.BancoDeDados.Migrations;
namespace Officio.Testes.Unitarios.InfraBancoDeDados;
public class MigracaoRepresentanteContratoTestes
{
    [Fact]
    public void Migration_PreservaContratosAntigosSemPreencherRepresentante()
    {
        var b = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        new Exposta().Aplicar(b);
        var coluna = Assert.Single(b.Operations.OfType<AddColumnOperation>());
        Assert.True(coluna.IsNullable); Assert.Null(coluna.DefaultValue);
        Assert.Equal("RepresentanteSocioId", coluna.Name);
        Assert.Empty(b.Operations.OfType<SqlOperation>());
        var fk = Assert.Single(b.Operations.OfType<AddForeignKeyOperation>());
        Assert.Equal("Socios", fk.PrincipalTable); Assert.Equal(ReferentialAction.Restrict, fk.OnDelete);
    }
    private class Exposta : AdicionarRepresentanteSocioContrato
    {
        public void Aplicar(MigrationBuilder b) => Up(b);
    }
}
