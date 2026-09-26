using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Officio.Infra.BancoDeDados.Contextos;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations;

[DbContext(typeof(ContextoAplicacao))]
[Migration("20260926020000_DefinirStatusInicialCliente")]
public partial class DefinirStatusInicialCliente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @statusClienteAtivoId uniqueidentifier =
                (SELECT TOP 1 [Id] FROM [StatusCliente] WHERE [Codigo] = 'ATIVO');

            IF @statusClienteAtivoId IS NULL
            BEGIN
                SET @statusClienteAtivoId = NEWID();

                INSERT INTO [StatusCliente] ([Id], [Ativo], [Codigo], [Descricao], [Ordem])
                VALUES (@statusClienteAtivoId, 1, 'ATIVO', 'Cliente ativo', 1);
            END;

            UPDATE [Clientes]
            SET [StatusClienteId] = @statusClienteAtivoId
            WHERE [StatusClienteId] IS NULL;
            """);

        migrationBuilder.DropForeignKey(
            name: "FK_Clientes_StatusCliente_StatusClienteId",
            table: "Clientes");

        migrationBuilder.DropIndex(
            name: "IX_Clientes_StatusClienteId",
            table: "Clientes");

        migrationBuilder.AlterColumn<Guid>(
            name: "StatusClienteId",
            table: "Clientes",
            type: "uniqueidentifier",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Clientes_StatusClienteId",
            table: "Clientes",
            column: "StatusClienteId");

        migrationBuilder.AddForeignKey(
            name: "FK_Clientes_StatusCliente_StatusClienteId",
            table: "Clientes",
            column: "StatusClienteId",
            principalTable: "StatusCliente",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Clientes_StatusCliente_StatusClienteId",
            table: "Clientes");

        migrationBuilder.DropIndex(
            name: "IX_Clientes_StatusClienteId",
            table: "Clientes");

        migrationBuilder.AlterColumn<Guid>(
            name: "StatusClienteId",
            table: "Clientes",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.CreateIndex(
            name: "IX_Clientes_StatusClienteId",
            table: "Clientes",
            column: "StatusClienteId");

        migrationBuilder.AddForeignKey(
            name: "FK_Clientes_StatusCliente_StatusClienteId",
            table: "Clientes",
            column: "StatusClienteId",
            principalTable: "StatusCliente",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
