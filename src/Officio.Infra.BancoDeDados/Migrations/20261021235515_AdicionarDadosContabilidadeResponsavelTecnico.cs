using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDadosContabilidadeResponsavelTecnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Crc",
                table: "Parceiros",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EnderecoId",
                table: "Parceiros",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Parceiros",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResponsaveisTecnicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParceiroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Profissao = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Crc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnderecoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsaveisTecnicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsaveisTecnicos_Enderecos_EnderecoId",
                        column: x => x.EnderecoId,
                        principalTable: "Enderecos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsaveisTecnicos_Parceiros_ParceiroId",
                        column: x => x.ParceiroId,
                        principalTable: "Parceiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parceiros_EnderecoId",
                table: "Parceiros",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsaveisTecnicos_EnderecoId",
                table: "ResponsaveisTecnicos",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsaveisTecnicos_ParceiroId",
                table: "ResponsaveisTecnicos",
                column: "ParceiroId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parceiros_Enderecos_EnderecoId",
                table: "Parceiros",
                column: "EnderecoId",
                principalTable: "Enderecos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parceiros_Enderecos_EnderecoId",
                table: "Parceiros");

            migrationBuilder.DropTable(
                name: "ResponsaveisTecnicos");

            migrationBuilder.DropIndex(
                name: "IX_Parceiros_EnderecoId",
                table: "Parceiros");

            migrationBuilder.DropColumn(
                name: "Crc",
                table: "Parceiros");

            migrationBuilder.DropColumn(
                name: "EnderecoId",
                table: "Parceiros");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Parceiros");
        }
    }
}
