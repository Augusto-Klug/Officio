using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIdempotenciaNotificacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [Notifications] WHERE DATALENGTH([Tipo]) > 160)
                    THROW 51000, 'Existem notificacoes com Tipo maior que 80 caracteres.', 1;

                IF EXISTS (SELECT 1 FROM [Notifications] WHERE DATALENGTH([Titulo]) > 400)
                    THROW 51000, 'Existem notificacoes com Titulo maior que 200 caracteres.', 1;

                IF EXISTS (SELECT 1 FROM [Notifications] WHERE DATALENGTH([Mensagem]) > 2000)
                    THROW 51000, 'Existem notificacoes com Mensagem maior que 1000 caracteres.', 1;

                ;WITH NotificacoesDuplicadas AS
                (
                    SELECT [Id],
                           ROW_NUMBER() OVER (
                               PARTITION BY [TenantId], [UserId], [Tipo], [ReferenciaId]
                               ORDER BY [CriadoEm], [Id]) AS [Ordem]
                    FROM [Notifications]
                    WHERE [ReferenciaId] IS NOT NULL
                )
                DELETE FROM NotificacoesDuplicadas WHERE [Ordem] > 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Notifications",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Mensagem",
                table: "Notifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantId_UserId_Tipo_ReferenciaId",
                table: "Notifications",
                columns: new[] { "TenantId", "UserId", "Tipo", "ReferenciaId" },
                unique: true,
                filter: "[ReferenciaId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_TenantId_UserId_Tipo_ReferenciaId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "Mensagem",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
