using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarSociosPessoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Participacao",
                table: "Socios",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<Guid>(
                name: "PessoaId",
                table: "Socios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoRelacaoId",
                table: "Socios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM Pessoas WHERE LEN(Profissao) > 250) THROW 51000, 'Profissao maior que 250 caracteres.', 1;");
            migrationBuilder.AlterColumn<string>(
                name: "Profissao",
                table: "Pessoas",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MunicipioId",
                table: "Enderecos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "EstadoId",
                table: "Enderecos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.DropIndex(name: "IX_Pessoas_Cpf", table: "Pessoas");
            migrationBuilder.CreateIndex(name: "IX_Pessoas_Cpf", table: "Pessoas", column: "Cpf");
            migrationBuilder.Sql("""
                -- Abort rather than truncate historical personal data.
                IF EXISTS (SELECT 1 FROM Socios WHERE LEN(Nome) > 250 OR LEN(REPLACE(REPLACE(REPLACE(REPLACE(Cpf, '.', ''), '-', ''), '/', ''), ' ', '')) > 11)
                    THROW 51000, 'Socios possuem Nome ou CPF maior que o limite de Pessoa. Corrija os registros antes de migrar.', 1;
                IF EXISTS (SELECT 1 FROM Pessoas WHERE LEN(Profissao) > 250)
                    THROW 51000, 'Profissao existente maior que 250 caracteres. Revise antes de migrar.', 1;

                IF NOT EXISTS (SELECT 1 FROM TiposRelacao WHERE Codigo = 'SOCIO')
                    INSERT INTO TiposRelacao (Id, Codigo, Descricao, Escopo, Ordem, Ativo)
                    VALUES (NEWID(), 'SOCIO', N'Sócio', 'SOCIETARIO', 0, 1);
                IF NOT EXISTS (SELECT 1 FROM TiposRelacao WHERE Codigo = 'SOCIO_ADMINISTRADOR')
                    INSERT INTO TiposRelacao (Id, Codigo, Descricao, Escopo, Ordem, Ativo)
                    VALUES (NEWID(), 'SOCIO_ADMINISTRADOR', N'Sócio administrador', 'SOCIETARIO', 1, 1);

                SELECT Id AS SocioId, NEWID() AS PessoaId INTO #SociosPessoa FROM Socios;
                -- Reuse a known person only inside the same prospect and with matching data.
                UPDATE m SET PessoaId = CONVERT(uniqueidentifier, candidato.Id)
                FROM #SociosPessoa m JOIN Socios s ON s.Id = m.SocioId
                CROSS APPLY (
                    SELECT MIN(CONVERT(varchar(36), p.Id)) AS Id
                    FROM PessoasProspecto pp JOIN Pessoas p ON p.Id = pp.PessoaId
                    WHERE pp.ProspectId = s.ProspectId
                      AND DATALENGTH(p.Nome) = DATALENGTH(s.Nome)
                      AND p.Nome COLLATE Latin1_General_100_BIN2 = s.Nome COLLATE Latin1_General_100_BIN2
                      AND p.Cpf = NULLIF(REPLACE(REPLACE(REPLACE(REPLACE(s.Cpf, '.', ''), '-', ''), '/', ''), ' ', ''), '')
                    HAVING COUNT(DISTINCT p.Id) = 1
                ) candidato WHERE candidato.Id IS NOT NULL;
                -- Only an unambiguous copy from the same tenant and originating prospect
                -- reuses the individual. CPF alone never merges unrelated records.
                UPDATE destino SET PessoaId = origem.PessoaId
                FROM #SociosPessoa destino
                JOIN Socios sc ON sc.Id = destino.SocioId
                JOIN Clientes c ON c.Id = sc.ClientId AND c.TenantId = sc.TenantId
                CROSS APPLY (
                    SELECT MIN(CONVERT(varchar(36), sp.Id)) AS Id
                    FROM Socios sp
                    WHERE sp.ProspectId = c.ProspectOrigemId AND sp.TenantId = sc.TenantId
                      AND DATALENGTH(sp.Nome) = DATALENGTH(sc.Nome)
                      AND sp.Nome COLLATE Latin1_General_100_BIN2 = sc.Nome COLLATE Latin1_General_100_BIN2
                      AND ISNULL(sp.Cpf, '') COLLATE Latin1_General_100_BIN2 = ISNULL(sc.Cpf, '') COLLATE Latin1_General_100_BIN2
                    HAVING COUNT(*) = 1
                ) candidato
                JOIN #SociosPessoa origem ON origem.SocioId = CONVERT(uniqueidentifier, candidato.Id);

                INSERT INTO Pessoas (Id, Nome, Cpf, CriadoEm, AtualizadoEm)
                SELECT m.PessoaId, s.Nome, NULLIF(REPLACE(REPLACE(REPLACE(REPLACE(s.Cpf, '.', ''), '-', ''), '/', ''), ' ', ''), ''), SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM #SociosPessoa m
                JOIN Socios s ON s.Id = m.SocioId
                WHERE NOT EXISTS (SELECT 1 FROM Pessoas p WHERE p.Id = m.PessoaId)
                  AND m.SocioId = (SELECT TOP (1) x.SocioId FROM #SociosPessoa x WHERE x.PessoaId = m.PessoaId ORDER BY x.SocioId);
                UPDATE s SET PessoaId = m.PessoaId FROM Socios s JOIN #SociosPessoa m ON m.SocioId = s.Id;
                DROP TABLE #SociosPessoa;
                """);
            migrationBuilder.AlterColumn<Guid>(name: "PessoaId", table: "Socios", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
            migrationBuilder.DropColumn(name: "Nome", table: "Socios");
            migrationBuilder.DropColumn(name: "Cpf", table: "Socios");

            migrationBuilder.CreateIndex(
                name: "IX_Socios_PessoaId",
                table: "Socios",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Socios_TipoRelacaoId",
                table: "Socios",
                column: "TipoRelacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_EstadoId",
                table: "Enderecos",
                column: "EstadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enderecos_Estados_EstadoId",
                table: "Enderecos",
                column: "EstadoId",
                principalTable: "Estados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Pessoas_PessoaId",
                table: "Socios",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_TiposRelacao_TipoRelacaoId",
                table: "Socios",
                column: "TipoRelacaoId",
                principalTable: "TiposRelacao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM Enderecos WHERE MunicipioId IS NULL)
                    THROW 51000, 'Complete os municipios dos enderecos parciais antes de reverter.', 1;
                IF EXISTS (SELECT Cpf FROM Pessoas WHERE Cpf IS NOT NULL GROUP BY Cpf HAVING COUNT(*) > 1)
                    THROW 51000, 'Existem pessoas distintas com o mesmo CPF; revise antes de restaurar o indice unico.', 1;
                """);
            migrationBuilder.AddColumn<string>(name: "Nome", table: "Socios", type: "nvarchar(max)", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "Cpf", table: "Socios", type: "nvarchar(max)", nullable: true);
            migrationBuilder.Sql("UPDATE s SET Nome = p.Nome, Cpf = p.Cpf FROM Socios s JOIN Pessoas p ON p.Id = s.PessoaId;");
            migrationBuilder.DropIndex(name: "IX_Pessoas_Cpf", table: "Pessoas");
            migrationBuilder.CreateIndex(name: "IX_Pessoas_Cpf", table: "Pessoas", column: "Cpf", unique: true, filter: "[Cpf] IS NOT NULL");

            migrationBuilder.DropForeignKey(
                name: "FK_Enderecos_Estados_EstadoId",
                table: "Enderecos");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Pessoas_PessoaId",
                table: "Socios");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_TiposRelacao_TipoRelacaoId",
                table: "Socios");

            migrationBuilder.DropIndex(
                name: "IX_Socios_PessoaId",
                table: "Socios");

            migrationBuilder.DropIndex(
                name: "IX_Socios_TipoRelacaoId",
                table: "Socios");

            migrationBuilder.DropIndex(
                name: "IX_Enderecos_EstadoId",
                table: "Enderecos");

            migrationBuilder.DropColumn(name: "PessoaId", table: "Socios");
            migrationBuilder.DropColumn(name: "TipoRelacaoId", table: "Socios");
            migrationBuilder.DropColumn(
                name: "EstadoId",
                table: "Enderecos");

            migrationBuilder.AlterColumn<decimal>(
                name: "Participacao",
                table: "Socios",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Profissao",
                table: "Pessoas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MunicipioId",
                table: "Enderecos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
