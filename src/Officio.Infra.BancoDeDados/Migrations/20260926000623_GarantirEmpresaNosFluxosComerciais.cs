using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class GarantirEmpresaNosFluxosComerciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [Parceiros] WHERE [Cnpj] IS NULL OR LTRIM(RTRIM([Cnpj])) = '')
                    THROW 51000, 'Existem parceiros sem CNPJ. Atualize-os antes de aplicar esta migration.', 1;

                IF EXISTS (SELECT 1 FROM [Clientes] WHERE [EmpresaId] IS NULL)
                    THROW 51000, 'Existem clientes sem empresa vinculada. Corrija os dados antes de aplicar esta migration.', 1;

                IF EXISTS (SELECT 1 FROM [Contratos] WHERE [EmpresaId] IS NULL)
                    THROW 51000, 'Existem contratos sem empresa vinculada. Corrija os dados antes de aplicar esta migration.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Parceiros_Cnpj",
                table: "Parceiros");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Parceiros",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmpresaId",
                table: "Contratos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmpresaId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parceiros_Cnpj",
                table: "Parceiros",
                column: "Cnpj",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Parceiros_Cnpj",
                table: "Parceiros");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Parceiros",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.AlterColumn<Guid>(
                name: "EmpresaId",
                table: "Contratos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EmpresaId",
                table: "Clientes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Parceiros_Cnpj",
                table: "Parceiros",
                column: "Cnpj",
                unique: true,
                filter: "[Cnpj] IS NOT NULL");
        }
    }
}
