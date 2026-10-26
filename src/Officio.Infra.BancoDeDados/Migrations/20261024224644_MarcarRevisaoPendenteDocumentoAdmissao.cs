using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class MarcarRevisaoPendenteDocumentoAdmissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RevisaoPendente",
                table: "DocumentosAdmissao",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevisaoPendente",
                table: "DocumentosAdmissao");
        }
    }
}
