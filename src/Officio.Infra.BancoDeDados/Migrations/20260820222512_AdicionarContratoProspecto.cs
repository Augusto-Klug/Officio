using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarContratoProspecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Budgets_OrcamentoId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_OrcamentoId",
                table: "Contracts");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "ProspectId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Contracts
                SET BudgetId = OrcamentoId
                WHERE BudgetId IS NULL
                  AND OrcamentoId IS NOT NULL
                """);

            migrationBuilder.Sql("""
                UPDATE C
                SET ProspectId = B.ProspectId
                FROM Contracts C
                INNER JOIN Budgets B ON B.Id = COALESCE(C.BudgetId, C.OrcamentoId)
                WHERE C.ProspectId IS NULL
                """);

            migrationBuilder.Sql("""
                UPDATE C
                SET ProspectId = CL.ProspectOrigemId
                FROM Contracts C
                INNER JOIN Clients CL ON CL.Id = C.ClientId
                WHERE C.ProspectId IS NULL
                  AND CL.ProspectOrigemId IS NOT NULL
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProspectId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "OrcamentoId",
                table: "Contracts");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BudgetId",
                table: "Contracts",
                column: "BudgetId",
                unique: true,
                filter: "[BudgetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProspectId",
                table: "Contracts",
                column: "ProspectId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TenantId_BudgetId",
                table: "Contracts",
                columns: new[] { "TenantId", "BudgetId" },
                unique: true,
                filter: "[BudgetId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Budgets_BudgetId",
                table: "Contracts",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Prospects_ProspectId",
                table: "Contracts",
                column: "ProspectId",
                principalTable: "Prospects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Budgets_BudgetId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Prospects_ProspectId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_BudgetId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ProspectId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_TenantId_BudgetId",
                table: "Contracts");

            migrationBuilder.AddColumn<Guid>(
                name: "OrcamentoId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Contracts
                SET OrcamentoId = BudgetId
                WHERE OrcamentoId IS NULL
                  AND BudgetId IS NOT NULL
                """);

            migrationBuilder.DropColumn(
                name: "ProspectId",
                table: "Contracts");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OrcamentoId",
                table: "Contracts",
                column: "OrcamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Budgets_OrcamentoId",
                table: "Contracts",
                column: "OrcamentoId",
                principalTable: "Budgets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
