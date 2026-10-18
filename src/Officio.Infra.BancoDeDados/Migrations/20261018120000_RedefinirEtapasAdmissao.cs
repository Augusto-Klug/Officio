using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Officio.Infra.BancoDeDados.Contextos;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations;

[DbContext(typeof(ContextoAplicacao))]
[Migration("20261018120000_RedefinirEtapasAdmissao")]
public partial class RedefinirEtapasAdmissao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        RemapearStatus(migrationBuilder, "AGUARDANDO_PREENCHIMENTO", "AGUARDANDO_ADMITIDO", "Aguardando admitido", 2);
        RemapearStatus(migrationBuilder, "EM_VALIDACAO", "AGUARDANDO_VALIDACAO", "Aguardando validacao", 3);
        RemapearStatus(migrationBuilder, "PENDENTE_REVISAO", "AGUARDANDO_VALIDACAO", "Aguardando validacao", 3);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        RemapearStatus(migrationBuilder, "AGUARDANDO_ADMITIDO", "AGUARDANDO_PREENCHIMENTO", "Aguardando preenchimento", 2);
        RemapearStatus(migrationBuilder, "AGUARDANDO_VALIDACAO", "EM_VALIDACAO", "Em validacao", 3);
        RemapearStatus(migrationBuilder, "DOCUMENTOS_VALIDADOS", "EM_VALIDACAO", "Em validacao", 3);
        RemapearStatus(migrationBuilder, "AGUARDANDO_EMPREGADOR", "EM_VALIDACAO", "Em validacao", 3);
        RemapearStatus(migrationBuilder, "LIBERADO_PELO_EMPREGADOR", "EM_VALIDACAO", "Em validacao", 3);
    }

    private static void RemapearStatus(
        MigrationBuilder migrationBuilder,
        string codigoOrigem,
        string codigoDestino,
        string descricaoDestino,
        int ordemDestino)
    {
        migrationBuilder.Sql($"""
            DECLARE @StatusOrigemId uniqueidentifier = (SELECT TOP(1) [Id] FROM [StatusAdmissao] WHERE [Codigo] = N'{codigoOrigem}');
            DECLARE @StatusDestinoId uniqueidentifier = (SELECT TOP(1) [Id] FROM [StatusAdmissao] WHERE [Codigo] = N'{codigoDestino}');

            IF @StatusOrigemId IS NOT NULL
            BEGIN
                IF @StatusDestinoId IS NULL
                BEGIN
                    UPDATE [StatusAdmissao]
                    SET [Codigo] = N'{codigoDestino}', [Descricao] = N'{descricaoDestino}', [Ordem] = {ordemDestino}
                    WHERE [Id] = @StatusOrigemId;
                END
                ELSE
                BEGIN
                    UPDATE [Admissoes] SET [StatusAdmissaoId] = @StatusDestinoId WHERE [StatusAdmissaoId] = @StatusOrigemId;
                    DELETE FROM [StatusAdmissao] WHERE [Id] = @StatusOrigemId;
                END
            END
            """);
    }
}
