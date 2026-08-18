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
            DECLARE @UsersObjectId int = OBJECT_ID(N'[dbo].[Users]', N'U');

            IF @UsersObjectId IS NULL
                THROW 51000, 'Migration incremental: a tabela [Users] nao existe. Aplique primeiro o schema base do Officio.', 1;

            IF COL_LENGTH(N'[dbo].[Users]', N'Email') IS NULL
                THROW 51000, 'Migration incremental: a coluna [Users].[Email] nao existe. Aplique primeiro o schema base do Officio.', 1;

            IF EXISTS
            (
                SELECT 1
                FROM sys.indexes
                WHERE object_id = @UsersObjectId
                  AND name = N'IX_Users_Email'
                  AND is_unique = 0
            )
                THROW 51000, 'O indice IX_Users_Email existente nao e unico. Remova ou renomeie o indice apos validar sua origem.', 1;

            EXEC sys.sp_executesql N'
                IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] IS NULL)
                    THROW 51000, ''Existem usuarios sem email. Preencha os valores antes de aplicar o indice unico.'', 1;

                IF EXISTS (SELECT 1 FROM [dbo].[Users] WHERE DATALENGTH([Email]) > 900)
                    THROW 51000, ''Existem emails com mais de 450 caracteres. Corrija esses registros antes de aplicar a migration.'', 1;

                IF EXISTS
                (
                    SELECT 1
                    FROM [dbo].[Users]
                    GROUP BY [Email]
                    HAVING COUNT(*) > 1
                )
                    THROW 51000, ''Existem emails duplicados. Saneie os usuarios antes de aplicar o indice unico.'', 1;

                DECLARE @TipoEmail sysname;
                DECLARE @TamanhoEmail smallint;
                DECLARE @EmailPermiteNulo bit;

                SELECT
                    @TipoEmail = tipo.name,
                    @TamanhoEmail = coluna.max_length,
                    @EmailPermiteNulo = coluna.is_nullable
                FROM sys.columns coluna
                INNER JOIN sys.types tipo ON tipo.user_type_id = coluna.user_type_id
                WHERE coluna.object_id = OBJECT_ID(N''[dbo].[Users]'', N''U'')
                  AND coluna.name = N''Email'';

                IF @TipoEmail <> N''nvarchar''
                   OR @TamanhoEmail <> 900
                   OR @EmailPermiteNulo = 1
                BEGIN
                    ALTER TABLE [dbo].[Users]
                        ALTER COLUMN [Email] nvarchar(450) NOT NULL;
                END

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N''[dbo].[Users]'', N''U'')
                      AND name = N''IX_Users_Email''
                      AND is_unique = 1
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]);
                END
            ';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DECLARE @UsersObjectId int = OBJECT_ID(N'[dbo].[Users]', N'U');

            IF @UsersObjectId IS NOT NULL
               AND EXISTS
               (
                   SELECT 1
                   FROM sys.indexes
                   WHERE object_id = @UsersObjectId
                     AND name = N'IX_Users_Email'
                     AND is_unique = 1
               )
            BEGIN
                EXEC sys.sp_executesql N'DROP INDEX [IX_Users_Email] ON [dbo].[Users];';
            END
            """);
    }
}
