using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AjustarDadosComerciaisOrcamentoContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "WITH OrcamentosAceitosOrdenados AS (" +
                " SELECT o.Id, ROW_NUMBER() OVER (PARTITION BY o.TenantId, o.ProspectId " +
                " ORDER BY CASE WHEN EXISTS (SELECT 1 FROM Contratos c WHERE c.BudgetId = o.Id) THEN 0 ELSE 1 END, " +
                " o.CriadoEm DESC, o.Id DESC) AS Ordem " +
                " FROM Orcamentos o WHERE o.Status = 'Aceito'" +
                ") " +
                "UPDATE o SET Status = 'Substituido' " +
                "FROM Orcamentos o INNER JOIN OrcamentosAceitosOrdenados a ON a.Id = o.Id " +
                "WHERE a.Ordem > 1;");

            migrationBuilder.AddColumn<DateOnly>(
                name: "CompetenciaInicialHonorarios",
                table: "Contratos",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuracaoAnos",
                table: "Contratos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TenantId_ProspectId_Aceito",
                table: "Orcamentos",
                columns: new[] { "TenantId", "ProspectId", "Status" },
                unique: true,
                filter: "[Status] = 'Aceito'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_TenantId_ProspectId_Aceito",
                table: "Orcamentos");

            migrationBuilder.DropColumn(
                name: "CompetenciaInicialHonorarios",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "DuracaoAnos",
                table: "Contratos");
        }
    }
}
