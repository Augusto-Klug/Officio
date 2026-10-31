using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class PermitirHistoricoContratosCancelados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contratos_BudgetId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_TenantId_BudgetId",
                table: "Contratos");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_BudgetId",
                table: "Contratos",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_TenantId_BudgetId",
                table: "Contratos",
                columns: new[] { "TenantId", "BudgetId" },
                unique: true,
                filter: "[BudgetId] IS NOT NULL AND [Status] <> 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contratos_BudgetId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_TenantId_BudgetId",
                table: "Contratos");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_BudgetId",
                table: "Contratos",
                column: "BudgetId",
                unique: true,
                filter: "[BudgetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_TenantId_BudgetId",
                table: "Contratos",
                columns: new[] { "TenantId", "BudgetId" },
                unique: true,
                filter: "[BudgetId] IS NOT NULL");
        }
    }
}
