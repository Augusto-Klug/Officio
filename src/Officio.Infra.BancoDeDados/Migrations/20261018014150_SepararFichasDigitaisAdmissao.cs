using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class SepararFichasDigitaisAdmissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenEmpresaAcessoHash",
                table: "Admissoes",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenEmpresaExpiraEm",
                table: "Admissoes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_TokenEmpresaAcessoHash",
                table: "Admissoes",
                column: "TokenEmpresaAcessoHash",
                unique: true,
                filter: "[TokenEmpresaAcessoHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Admissoes_TokenEmpresaAcessoHash",
                table: "Admissoes");

            migrationBuilder.DropColumn(
                name: "TokenEmpresaAcessoHash",
                table: "Admissoes");

            migrationBuilder.DropColumn(
                name: "TokenEmpresaExpiraEm",
                table: "Admissoes");
        }
    }
}
