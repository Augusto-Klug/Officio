using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Officio.Infra.BancoDeDados.Contextos;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations;

[DbContext(typeof(ContextoAplicacao))]
[Migration("20260926040000_NormalizarEnderecoCliente")]
public partial class NormalizarEnderecoCliente : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "EnderecoId",
            table: "Clientes",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Clientes_EnderecoId",
            table: "Clientes",
            column: "EnderecoId");

        migrationBuilder.AddForeignKey(
            name: "FK_Clientes_Enderecos_EnderecoId",
            table: "Clientes",
            column: "EnderecoId",
            principalTable: "Enderecos",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Clientes_Enderecos_EnderecoId", table: "Clientes");
        migrationBuilder.DropIndex(name: "IX_Clientes_EnderecoId", table: "Clientes");
        migrationBuilder.DropColumn(name: "EnderecoId", table: "Clientes");
    }
}
