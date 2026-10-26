using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRepresentanteSocioContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RepresentanteSocioId",
                table: "Contratos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_RepresentanteSocioId",
                table: "Contratos",
                column: "RepresentanteSocioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Socios_RepresentanteSocioId",
                table: "Contratos",
                column: "RepresentanteSocioId",
                principalTable: "Socios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Socios_RepresentanteSocioId",
                table: "Contratos");

            migrationBuilder.DropIndex(
                name: "IX_Contratos_RepresentanteSocioId",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "RepresentanteSocioId",
                table: "Contratos");
        }
    }
}
