using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class IntegrarLocalidadesIbgeViaCep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoIbge = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Sigla = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    MunicipiosSincronizadosEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Estados",
                columns: new[] { "Id", "CodigoIbge", "Sigla", "Nome", "Ativo" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000011"), "11", "RO", "Rondônia", true },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "12", "AC", "Acre", true },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "13", "AM", "Amazonas", true },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "14", "RR", "Roraima", true },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "15", "PA", "Pará", true },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "16", "AP", "Amapá", true },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "17", "TO", "Tocantins", true },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "21", "MA", "Maranhão", true },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "22", "PI", "Piauí", true },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "23", "CE", "Ceará", true },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "24", "RN", "Rio Grande do Norte", true },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "25", "PB", "Paraíba", true },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "26", "PE", "Pernambuco", true },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "27", "AL", "Alagoas", true },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "28", "SE", "Sergipe", true },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "29", "BA", "Bahia", true },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "31", "MG", "Minas Gerais", true },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "32", "ES", "Espírito Santo", true },
                    { new Guid("00000000-0000-0000-0000-000000000033"), "33", "RJ", "Rio de Janeiro", true },
                    { new Guid("00000000-0000-0000-0000-000000000035"), "35", "SP", "São Paulo", true },
                    { new Guid("00000000-0000-0000-0000-000000000041"), "41", "PR", "Paraná", true },
                    { new Guid("00000000-0000-0000-0000-000000000042"), "42", "SC", "Santa Catarina", true },
                    { new Guid("00000000-0000-0000-0000-000000000043"), "43", "RS", "Rio Grande do Sul", true },
                    { new Guid("00000000-0000-0000-0000-000000000050"), "50", "MS", "Mato Grosso do Sul", true },
                    { new Guid("00000000-0000-0000-0000-000000000051"), "51", "MT", "Mato Grosso", true },
                    { new Guid("00000000-0000-0000-0000-000000000052"), "52", "GO", "Goiás", true },
                    { new Guid("00000000-0000-0000-0000-000000000053"), "53", "DF", "Distrito Federal", true }
                });

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Prospectos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MunicipioId",
                table: "Prospectos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EstadoId",
                table: "Municipios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE municipio
                SET municipio.EstadoId = estado.Id
                FROM Municipios municipio
                INNER JOIN Estados estado ON estado.Sigla = UPPER(LTRIM(RTRIM(municipio.Uf)));

                UPDATE prospecto
                SET prospecto.MunicipioId = municipio.Id
                FROM Prospectos prospecto
                INNER JOIN Municipios municipio
                    ON municipio.Uf = UPPER(LTRIM(RTRIM(prospecto.Uf)))
                   AND municipio.Nome = LTRIM(RTRIM(prospecto.Cidade))
                WHERE prospecto.Cidade IS NOT NULL AND prospecto.Uf IS NOT NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "EstadoId",
                table: "Municipios",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(name: "Uf", table: "Municipios");

            migrationBuilder.CreateIndex(
                name: "IX_Prospectos_MunicipioId",
                table: "Prospectos",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipios_EstadoId_Nome",
                table: "Municipios",
                columns: new[] { "EstadoId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_Estados_CodigoIbge",
                table: "Estados",
                column: "CodigoIbge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estados_Sigla",
                table: "Estados",
                column: "Sigla",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Municipios_Estados_EstadoId",
                table: "Municipios",
                column: "EstadoId",
                principalTable: "Estados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prospectos_Municipios_MunicipioId",
                table: "Prospectos",
                column: "MunicipioId",
                principalTable: "Municipios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prospectos_Municipios_MunicipioId",
                table: "Prospectos");

            migrationBuilder.DropIndex(
                name: "IX_Prospectos_MunicipioId",
                table: "Prospectos");

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "Municipios",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE municipio
                SET municipio.Uf = estado.Sigla
                FROM Municipios municipio
                INNER JOIN Estados estado ON estado.Id = municipio.EstadoId;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Municipios_Estados_EstadoId",
                table: "Municipios");

            migrationBuilder.DropIndex(
                name: "IX_Municipios_EstadoId_Nome",
                table: "Municipios");

            migrationBuilder.DropColumn(name: "Complemento", table: "Prospectos");
            migrationBuilder.DropColumn(name: "MunicipioId", table: "Prospectos");
            migrationBuilder.DropColumn(name: "EstadoId", table: "Municipios");

            migrationBuilder.DropTable(name: "Estados");
        }
    }
}
