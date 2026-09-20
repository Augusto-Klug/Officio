using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTokenFichaAdmissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Admissoes]
                SET [TokenAcessoHash] = NULL, [TokenExpiraEm] = NULL
                WHERE [TokenAcessoHash] IS NOT NULL AND LEN([TokenAcessoHash]) <> 64;

                WITH [TokensDuplicados] AS
                (
                    SELECT [Id], ROW_NUMBER() OVER (
                        PARTITION BY [TokenAcessoHash]
                        ORDER BY [Id]) AS [Ordem]
                    FROM [Admissoes]
                    WHERE [TokenAcessoHash] IS NOT NULL
                )
                UPDATE [Admissoes]
                SET [TokenAcessoHash] = NULL, [TokenExpiraEm] = NULL
                WHERE [Id] IN (SELECT [Id] FROM [TokensDuplicados] WHERE [Ordem] > 1);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "TokenAcessoHash",
                table: "Admissoes",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_TokenAcessoHash",
                table: "Admissoes",
                column: "TokenAcessoHash",
                unique: true,
                filter: "[TokenAcessoHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Admissoes_TokenAcessoHash",
                table: "Admissoes");

            migrationBuilder.AlterColumn<string>(
                name: "TokenAcessoHash",
                table: "Admissoes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
