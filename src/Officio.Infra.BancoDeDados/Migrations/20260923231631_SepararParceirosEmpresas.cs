using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class SepararParceirosEmpresas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Empresas_Cnpj",
                table: "Empresas");

            migrationBuilder.AddColumn<Guid>(
                name: "CriadoPorId",
                table: "Empresas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParceiroId",
                table: "Empresas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Parceiros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RazaoSocial = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NomeFantasia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Cnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parceiros", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                CREATE TABLE #ParceiroIds
                (
                    Id uniqueidentifier NOT NULL PRIMARY KEY
                );

                DECLARE @coletarParceiros nvarchar(max) = N'';

                SELECT @coletarParceiros = @coletarParceiros +
                    N'INSERT INTO #ParceiroIds (Id) ' +
                    N'SELECT DISTINCT origem.[TenantId] FROM ' + QUOTENAME(schemaTabela.[name]) + N'.' + QUOTENAME(tabela.[name]) + N' origem ' +
                    N'WHERE origem.[TenantId] IS NOT NULL ' +
                    N'AND origem.[TenantId] <> ''00000000-0000-0000-0000-000000000000'' ' +
                    N'AND NOT EXISTS (SELECT 1 FROM #ParceiroIds existente WHERE existente.Id = origem.[TenantId]);'
                FROM sys.tables tabela
                INNER JOIN sys.schemas schemaTabela ON schemaTabela.[schema_id] = tabela.[schema_id]
                WHERE EXISTS
                (
                    SELECT 1
                    FROM sys.columns coluna
                    WHERE coluna.[object_id] = tabela.[object_id]
                      AND coluna.[name] = N'TenantId'
                );

                EXEC sp_executesql @coletarParceiros;

                IF EXISTS
                (
                    SELECT 1
                    FROM [Usuarios]
                    WHERE [TenantId] = '00000000-0000-0000-0000-000000000000'
                )
                    THROW 51000, 'Separacao de parceiros: existe usuario com TenantId vazio.', 1;

                IF EXISTS
                (
                    SELECT 1
                    FROM [ConvitesUsuarios]
                    WHERE [TenantId] = '00000000-0000-0000-0000-000000000000'
                )
                    THROW 51000, 'Separacao de parceiros: existe convite com TenantId vazio.', 1;

                INSERT INTO [Parceiros]
                (
                    [Id], [Nome], [RazaoSocial], [NomeFantasia], [Cnpj], [Email],
                    [Ativo], [CriadoEm], [AtualizadoEm]
                )
                SELECT
                    parceiro.Id,
                    COALESCE(NULLIF(empresa.[Nome], ''), NULLIF(empresa.[RazaoSocial], ''), CONCAT('Parceiro ', CONVERT(nvarchar(36), parceiro.Id))),
                    COALESCE(NULLIF(empresa.[RazaoSocial], ''), NULLIF(empresa.[Nome], ''), CONCAT('Parceiro ', CONVERT(nvarchar(36), parceiro.Id))),
                    NULLIF(empresa.[NomeFantasia], ''),
                    NULLIF(empresa.[Cnpj], ''),
                    NULLIF(empresa.[Email], ''),
                    COALESCE(empresa.[Ativo], 1),
                    COALESCE(empresa.[CriadoEm], SYSUTCDATETIME()),
                    COALESCE(empresa.[AtualizadoEm], SYSUTCDATETIME())
                FROM #ParceiroIds parceiro
                LEFT JOIN [Empresas] empresa ON empresa.[Id] = parceiro.Id;

                CREATE TABLE #EmpresaParceiroCandidatos
                (
                    EmpresaId uniqueidentifier NOT NULL,
                    ParceiroId uniqueidentifier NOT NULL
                );

                DECLARE @coletarVinculos nvarchar(max) = N'';

                SELECT @coletarVinculos = @coletarVinculos +
                    N'INSERT INTO #EmpresaParceiroCandidatos (EmpresaId, ParceiroId) ' +
                    N'SELECT DISTINCT origem.[EmpresaId], origem.[TenantId] FROM ' + QUOTENAME(schemaTabela.[name]) + N'.' + QUOTENAME(tabela.[name]) + N' origem ' +
                    N'WHERE origem.[EmpresaId] IS NOT NULL AND origem.[TenantId] IS NOT NULL;'
                FROM sys.tables tabela
                INNER JOIN sys.schemas schemaTabela ON schemaTabela.[schema_id] = tabela.[schema_id]
                WHERE EXISTS
                (
                    SELECT 1 FROM sys.columns coluna
                    WHERE coluna.[object_id] = tabela.[object_id] AND coluna.[name] = N'EmpresaId'
                )
                AND EXISTS
                (
                    SELECT 1 FROM sys.columns coluna
                    WHERE coluna.[object_id] = tabela.[object_id] AND coluna.[name] = N'TenantId'
                );

                EXEC sp_executesql @coletarVinculos;

                IF EXISTS
                (
                    SELECT candidato.EmpresaId
                    FROM #EmpresaParceiroCandidatos candidato
                    GROUP BY candidato.EmpresaId
                    HAVING COUNT(DISTINCT candidato.ParceiroId) > 1
                )
                    THROW 51000, 'Separacao de parceiros: uma empresa esta vinculada a mais de um parceiro.', 1;

                UPDATE empresa
                SET [ParceiroId] = candidato.ParceiroId
                FROM [Empresas] empresa
                INNER JOIN
                (
                    SELECT DISTINCT EmpresaId, ParceiroId
                    FROM #EmpresaParceiroCandidatos
                ) candidato ON candidato.EmpresaId = empresa.Id;

                DELETE usuarioEmpresa
                FROM [UsuariosEmpresa] usuarioEmpresa
                INNER JOIN #ParceiroIds parceiro ON parceiro.Id = usuarioEmpresa.[EmpresaId]
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM #EmpresaParceiroCandidatos candidato
                    WHERE candidato.EmpresaId = usuarioEmpresa.[EmpresaId]
                );

                DELETE empresa
                FROM [Empresas] empresa
                INNER JOIN #ParceiroIds parceiro ON parceiro.Id = empresa.Id
                WHERE empresa.[ParceiroId] IS NULL;

                IF EXISTS (SELECT 1 FROM [Empresas] WHERE [ParceiroId] IS NULL)
                    THROW 51000, 'Separacao de parceiros: existem empresas sem vinculo que permita identificar o parceiro.', 1;

                DROP TABLE #EmpresaParceiroCandidatos;
                DROP TABLE #ParceiroIds;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ParceiroId",
                table: "Empresas",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId",
                table: "Usuarios",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_ParceiroId_Cnpj",
                table: "Empresas",
                columns: new[] { "ParceiroId", "Cnpj" },
                unique: true,
                filter: "[Cnpj] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Parceiros_Cnpj",
                table: "Parceiros",
                column: "Cnpj",
                unique: true,
                filter: "[Cnpj] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ConvitesUsuarios_Parceiros_TenantId",
                table: "ConvitesUsuarios",
                column: "TenantId",
                principalTable: "Parceiros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Empresas_Parceiros_ParceiroId",
                table: "Empresas",
                column: "ParceiroId",
                principalTable: "Parceiros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Parceiros_TenantId",
                table: "Usuarios",
                column: "TenantId",
                principalTable: "Parceiros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConvitesUsuarios_Parceiros_TenantId",
                table: "ConvitesUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Empresas_Parceiros_ParceiroId",
                table: "Empresas");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Parceiros_TenantId",
                table: "Usuarios");

            migrationBuilder.Sql(
                """
                INSERT INTO [Empresas]
                (
                    [Id], [EnderecoId], [Nome], [Cnpj], [RazaoSocial], [NomeFantasia],
                    [InscricaoEstadual], [InscricaoMunicipal], [Telefone], [Email],
                    [TipoEmpresa], [RegimeTributario], [QuantidadeFuncionariosAtual],
                    [QuantidadeProLaboresAtual], [Ativo], [CriadoEm], [AtualizadoEm],
                    [CriadoPorId], [ParceiroId]
                )
                SELECT
                    parceiro.[Id], NULL, parceiro.[Nome], COALESCE(parceiro.[Cnpj], ''),
                    parceiro.[RazaoSocial], parceiro.[NomeFantasia], NULL, NULL, NULL,
                    parceiro.[Email], NULL, NULL, 0, 0, parceiro.[Ativo],
                    parceiro.[CriadoEm], parceiro.[AtualizadoEm], NULL, parceiro.[Id]
                FROM [Parceiros] parceiro
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM [Empresas] empresa WHERE empresa.[Id] = parceiro.[Id]
                );
                """);

            migrationBuilder.DropTable(
                name: "Parceiros");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TenantId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Empresas_ParceiroId_Cnpj",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "CriadoPorId",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ParceiroId",
                table: "Empresas");

            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT [Cnpj]
                    FROM [Empresas]
                    WHERE [Cnpj] <> ''
                    GROUP BY [Cnpj]
                    HAVING COUNT(*) > 1
                )
                    THROW 51000, 'Reversao da separacao de parceiros: existem CNPJs repetidos entre parceiros.', 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Cnpj",
                table: "Empresas",
                column: "Cnpj",
                unique: true,
                filter: "[Cnpj] <> ''");
        }
    }
}
