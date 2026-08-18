using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Migrations;

[DbContext(typeof(ContextoAplicacao))]
[Migration("20260818003714_AdicionarIndiceUnicoEmailUsuario")]
public partial class AdicionarIndiceUnicoEmailUsuario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[Users]', N'U') IS NOT NULL
               AND NOT EXISTS
               (
                   SELECT 1
                   FROM sys.indexes
                   WHERE name = N'IX_Users_Email'
                     AND object_id = OBJECT_ID(N'[Users]', N'U')
               )
            BEGIN
                CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[Users]', N'U') IS NOT NULL
               AND EXISTS
               (
                   SELECT 1
                   FROM sys.indexes
                   WHERE name = N'IX_Users_Email'
                     AND object_id = OBJECT_ID(N'[Users]', N'U')
               )
            BEGIN
                DROP INDEX [IX_Users_Email] ON [Users];
            END
            """);
    }
}
