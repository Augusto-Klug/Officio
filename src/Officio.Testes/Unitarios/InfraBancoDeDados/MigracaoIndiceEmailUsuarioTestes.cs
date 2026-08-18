using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Officio.Infra.BancoDeDados.Migrations;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class MigracaoIndiceEmailUsuarioTestes
{
    [Fact]
    public void Up_ProtegeUpgradeIncrementalAntesDeCriarIndice()
    {
        var construtor = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        new MigracaoExposta().Aplicar(construtor);

        var sql = Assert.Single(construtor.Operations.OfType<SqlOperation>()).Sql;
        Assert.Contains("THROW", sql);
        Assert.Contains("sp_executesql", sql);
        Assert.Contains("DATALENGTH([Email]) > 900", sql);
        Assert.DoesNotContain("LEN([Email])", sql);
        Assert.Contains("ALTER COLUMN [Email] nvarchar(450) NOT NULL", sql);
        Assert.Contains("HAVING COUNT(*) > 1", sql);
        Assert.Contains("is_unique = 0", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Users_Email]", sql);
    }

    [Fact]
    public void Down_RemoveSomenteIndiceUnicoQuandoExistir()
    {
        var construtor = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");

        new MigracaoExposta().Reverter(construtor);

        var sql = Assert.Single(construtor.Operations.OfType<SqlOperation>()).Sql;
        Assert.Contains("is_unique = 1", sql);
        Assert.Contains("DROP INDEX [IX_Users_Email]", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
    }

    private sealed class MigracaoExposta : AdicionarIndiceUnicoEmailUsuario
    {
        public void Aplicar(MigrationBuilder construtor) => Up(construtor);
        public void Reverter(MigrationBuilder construtor) => Down(construtor);
    }
}
