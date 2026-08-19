using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarOrcamentosCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_Budgets_OrcamentoId",
                table: "BudgetItems");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Budgets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusNovo",
                table: "Budgets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Rascunho");

            migrationBuilder.Sql(
                "UPDATE Budgets SET StatusNovo = CASE Status " +
                "WHEN 0 THEN 'Rascunho' " +
                "WHEN 1 THEN 'Enviado' " +
                "WHEN 2 THEN 'Aceito' " +
                "WHEN 3 THEN 'Recusado' " +
                "WHEN 4 THEN 'Expirado' " +
                "WHEN 5 THEN 'Aceito' " +
                "ELSE 'Rascunho' END");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Budgets");

            migrationBuilder.RenameColumn(
                name: "StatusNovo",
                table: "Budgets",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Budgets",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRecusa",
                table: "Budgets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE BudgetItems SET OrcamentoId = BudgetId " +
                "WHERE OrcamentoId IS NULL");

            migrationBuilder.Sql(
                "UPDATE BudgetItems SET Tipo = CASE " +
                "WHEN Tipo IN ('ServicoDescricao', 'ItemMonetario', 'RegraComercial') THEN Tipo " +
                "WHEN ValorUnitario > 0 THEN 'ItemMonetario' " +
                "ELSE 'ServicoDescricao' END");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "BudgetItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ServicoDescricao",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrcamentoId",
                table: "BudgetItems",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "BudgetItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "ItemCatalogoOrcamentoId",
                table: "BudgetItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "BudgetItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Secao",
                table: "BudgetItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ServicosInclusos");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "BudgetItems");

            migrationBuilder.CreateTable(
                name: "BudgetCatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Secao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValorReferencia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TenantId_ProspectId",
                table: "Budgets",
                columns: new[] { "TenantId", "ProspectId" });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_TenantId_OrcamentoId_Ordem",
                table: "BudgetItems",
                columns: new[] { "TenantId", "OrcamentoId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCatalogItems_TenantId_Ativo_Secao",
                table: "BudgetCatalogItems",
                columns: new[] { "TenantId", "Ativo", "Secao" });

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_Budgets_OrcamentoId",
                table: "BudgetItems",
                column: "OrcamentoId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_Budgets_OrcamentoId",
                table: "BudgetItems");

            migrationBuilder.DropTable(
                name: "BudgetCatalogItems");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_TenantId_ProspectId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_BudgetItems_TenantId_OrcamentoId_Ordem",
                table: "BudgetItems");

            migrationBuilder.DropColumn(
                name: "MotivoRecusa",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "ItemCatalogoOrcamentoId",
                table: "BudgetItems");

            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "BudgetItems");

            migrationBuilder.DropColumn(
                name: "Secao",
                table: "BudgetItems");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Budgets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusLegado",
                table: "Budgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE Budgets SET StatusLegado = CASE Status " +
                "WHEN 'Enviado' THEN 1 " +
                "WHEN 'Aceito' THEN 2 " +
                "WHEN 'Recusado' THEN 3 " +
                "WHEN 'Expirado' THEN 4 " +
                "ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Budgets");

            migrationBuilder.RenameColumn(
                name: "StatusLegado",
                table: "Budgets",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Budgets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "BudgetItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrcamentoId",
                table: "BudgetItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "BudgetItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetId",
                table: "BudgetItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                "UPDATE BudgetItems SET BudgetId = OrcamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_Budgets_OrcamentoId",
                table: "BudgetItems",
                column: "OrcamentoId",
                principalTable: "Budgets",
                principalColumn: "Id");
        }
    }
}
