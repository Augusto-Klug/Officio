using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDiagnosticosDetalhadosOcr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriaResultado",
                table: "ExecucoesOcr",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusHttp",
                table: "ExecucoesOcr",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaResultado",
                table: "ExecucoesOcr");

            migrationBuilder.DropColumn(
                name: "StatusHttp",
                table: "ExecucoesOcr");
        }
    }
}
