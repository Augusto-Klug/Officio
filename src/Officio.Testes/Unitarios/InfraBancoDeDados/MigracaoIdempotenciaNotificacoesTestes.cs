using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Officio.Infra.BancoDeDados.Migrations;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class MigracaoIdempotenciaNotificacoesTestes
{
    [Fact]
    public void Up_ValidaLimitesRemoveDuplicidadesECriaIndiceUnicoFiltrado()
    {
        var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        new MigracaoExposta().AplicarUp(migrationBuilder);

        var sql = Assert.IsType<SqlOperation>(migrationBuilder.Operations[0]).Sql;
        Assert.Contains("DATALENGTH([Tipo]) > 160", sql);
        Assert.Contains("DATALENGTH([Titulo]) > 400", sql);
        Assert.Contains("DATALENGTH([Mensagem]) > 2000", sql);
        Assert.Contains("ROW_NUMBER() OVER", sql);
        Assert.Contains("DELETE FROM NotificacoesDuplicadas", sql);

        var indice = Assert.Single(migrationBuilder.Operations.OfType<CreateIndexOperation>());
        Assert.True(indice.IsUnique);
        Assert.Equal("IX_Notifications_TenantId_UserId_Tipo_ReferenciaId", indice.Name);
        Assert.Equal("[ReferenciaId] IS NOT NULL", indice.Filter);
    }

    private sealed class MigracaoExposta : AdicionarIdempotenciaNotificacoes
    {
        public void AplicarUp(MigrationBuilder migrationBuilder)
            => Up(migrationBuilder);
    }
}
