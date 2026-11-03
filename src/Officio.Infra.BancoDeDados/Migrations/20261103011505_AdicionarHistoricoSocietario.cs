using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarHistoricoSocietario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Socios",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VigenteAte",
                table: "Socios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VigenteDesde",
                table: "Socios",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.Sql("UPDATE [Socios] SET [VigenteDesde] = [CriadoEm]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "VigenteAte",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "VigenteDesde",
                table: "Socios");
        }
    }
}
