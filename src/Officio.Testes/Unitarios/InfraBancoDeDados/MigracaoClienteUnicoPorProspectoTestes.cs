using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Officio.Infra.BancoDeDados.Migrations;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class MigracaoClienteUnicoPorProspectoTestes
{
    [Fact]
    public void Up_ValidaDuplicidadesECriaIndiceUnicoFiltrado()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        new MigracaoExposta().AplicarUp(migrationBuilder);

        var sql = Assert.IsType<SqlOperation>(migrationBuilder.Operations[0]).Sql;
        Assert.Contains("HAVING COUNT(*) > 1", sql);
        Assert.Contains("THROW 51000", sql);

        var indice = Assert.Single(migrationBuilder.Operations.OfType<CreateIndexOperation>());
        Assert.True(indice.IsUnique);
        Assert.Equal("IX_Clients_TenantId_ProspectOrigemId", indice.Name);
        Assert.Equal("[ProspectOrigemId] IS NOT NULL", indice.Filter);
    }

    private sealed class MigracaoExposta : GarantirClienteUnicoPorProspecto
    {
        public void AplicarUp(MigrationBuilder migrationBuilder)
            => Up(migrationBuilder);
    }
}
