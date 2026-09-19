using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class GarantirClienteUnicoPorProspecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [Clients]
                    WHERE [ProspectOrigemId] IS NOT NULL
                    GROUP BY [TenantId], [ProspectOrigemId]
                    HAVING COUNT(*) > 1
                )
                    THROW 51000, 'Existem clientes duplicados para o mesmo prospecto de origem. Corrija os dados antes de aplicar a migration.', 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_ProspectOrigemId",
                table: "Clients",
                columns: new[] { "TenantId", "ProspectOrigemId" },
                unique: true,
                filter: "[ProspectOrigemId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_TenantId_ProspectOrigemId",
                table: "Clients");
        }
    }
}
