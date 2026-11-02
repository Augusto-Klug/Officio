using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class PortalClienteComVinculoEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "ConvitesUsuarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConvitesUsuarios_EmpresaId",
                table: "ConvitesUsuarios",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConvitesUsuarios_Empresas_EmpresaId",
                table: "ConvitesUsuarios",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConvitesUsuarios_Empresas_EmpresaId",
                table: "ConvitesUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_ConvitesUsuarios_EmpresaId",
                table: "ConvitesUsuarios");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "ConvitesUsuarios");

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
    }
}
