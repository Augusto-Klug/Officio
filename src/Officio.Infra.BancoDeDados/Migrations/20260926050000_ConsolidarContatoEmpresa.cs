using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Officio.Infra.BancoDeDados.Contextos;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations;

[DbContext(typeof(ContextoAplicacao))]
[Migration("20260926050000_ConsolidarContatoEmpresa")]
public partial class ConsolidarContatoEmpresa : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Whatsapp",
            table: "Empresas",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE empresa
            SET empresa.RazaoSocial = CASE
                    WHEN empresa.RazaoSocial = '' THEN cliente.RazaoSocial
                    ELSE empresa.RazaoSocial
                END,
                empresa.Nome = CASE
                    WHEN empresa.Nome = '' THEN cliente.RazaoSocial
                    ELSE empresa.Nome
                END,
                empresa.Cnpj = CASE
                    WHEN empresa.Cnpj = '' AND cliente.Cnpj IS NOT NULL THEN cliente.Cnpj
                    ELSE empresa.Cnpj
                END,
                empresa.NomeFantasia = COALESCE(empresa.NomeFantasia, cliente.NomeFantasia),
                empresa.Email = COALESCE(empresa.Email, cliente.Email),
                empresa.Telefone = COALESCE(empresa.Telefone, cliente.Telefone),
                empresa.RegimeTributario = COALESCE(
                    empresa.RegimeTributario,
                    CASE cliente.RegimeTributario
                        WHEN 0 THEN 'SimplesNacional'
                        WHEN 1 THEN 'LucroPresumido'
                        WHEN 2 THEN 'LucroReal'
                        WHEN 3 THEN 'MEI'
                    END),
                empresa.Whatsapp = COALESCE(empresa.Whatsapp, cliente.Whatsapp)
            FROM Empresas empresa
            INNER JOIN Clientes cliente ON cliente.EmpresaId = empresa.Id
            WHERE empresa.RazaoSocial = ''
               OR empresa.Nome = ''
               OR empresa.Cnpj = ''
               OR empresa.NomeFantasia IS NULL
               OR empresa.Email IS NULL
               OR empresa.Telefone IS NULL
               OR empresa.RegimeTributario IS NULL
               OR (empresa.Whatsapp IS NULL AND cliente.Whatsapp IS NOT NULL);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropColumn(name: "Whatsapp", table: "Empresas");
}
