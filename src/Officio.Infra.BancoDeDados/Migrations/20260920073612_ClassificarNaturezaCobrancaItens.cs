using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class ClassificarNaturezaCobrancaItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NaturezaCobranca",
                table: "BudgetItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "NaoRecorrente");

            migrationBuilder.AddColumn<string>(
                name: "NaturezaCobranca",
                table: "BudgetCatalogItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "NaoRecorrente");

            migrationBuilder.Sql(
                """
                UPDATE [BudgetItems]
                SET [NaturezaCobranca] = N'HonorarioMensal'
                WHERE [Secao] = N'Honorarios' AND [Tipo] = N'ItemMonetario';

                UPDATE [BudgetCatalogItems]
                SET [NaturezaCobranca] = N'HonorarioMensal'
                WHERE [Secao] = N'Honorarios' AND [Tipo] = N'ItemMonetario';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NaturezaCobranca",
                table: "BudgetItems");

            migrationBuilder.DropColumn(
                name: "NaturezaCobranca",
                table: "BudgetCatalogItems");
        }
    }
}
