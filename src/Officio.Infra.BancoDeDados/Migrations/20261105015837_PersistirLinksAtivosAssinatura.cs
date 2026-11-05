using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class PersistirLinksAtivosAssinatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkAssinatura",
                table: "SignatariosAssinatura",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkAssinatura",
                table: "SignatariosAssinatura");
        }
    }
}
