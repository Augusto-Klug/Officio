using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Officio.Infra.BancoDeDados.Contextos;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations;

[DbContext(typeof(ContextoAplicacao))]
[Migration("20260926030000_AdicionarDadosCadastraisProspecto")]
public partial class AdicionarDadosCadastraisProspecto : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "InscricaoEstadual",
            table: "Prospectos",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "InscricaoMunicipal",
            table: "Prospectos",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TipoEmpresa",
            table: "Prospectos",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "InscricaoEstadual", table: "Prospectos");
        migrationBuilder.DropColumn(name: "InscricaoMunicipal", table: "Prospectos");
        migrationBuilder.DropColumn(name: "TipoEmpresa", table: "Prospectos");
    }
}
