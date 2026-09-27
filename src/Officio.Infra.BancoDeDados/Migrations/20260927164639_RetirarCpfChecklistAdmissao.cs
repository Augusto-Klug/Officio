using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class RetirarCpfChecklistAdmissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @CpfId uniqueidentifier = (
                    SELECT TOP(1) [Id] FROM [TiposDocumento] WHERE [Codigo] = N'CPF'
                );

                UPDATE [ConfiguracoesDocumentoAdmissao]
                SET [Ativo] = 0
                WHERE [TipoDocumentoId] = @CpfId;

                DELETE FROM [DocumentosAdmissao]
                WHERE [TipoDocumentoId] = @CpfId
                  AND [DocumentoId] IS NULL;

                UPDATE configuracao
                SET [Ordem] = ordem.NovaOrdem
                FROM [ConfiguracoesDocumentoAdmissao] configuracao
                INNER JOIN [TiposDocumento] tipo ON tipo.Id = configuracao.TipoDocumentoId
                INNER JOIN (VALUES
                    (N'IDENTIDADE', 1),
                    (N'COMPROVANTE_RESIDENCIA', 2),
                    (N'CARTEIRA_TRABALHO', 3),
                    (N'CNH', 4),
                    (N'TITULO_ELEITOR', 5),
                    (N'COMPROVANTE_ESCOLARIDADE', 6),
                    (N'CERTIFICADO_RESERVISTA', 7)
                ) ordem(Codigo, NovaOrdem) ON ordem.Codigo = tipo.Codigo;

                UPDATE item
                SET [Ordem] = ordem.NovaOrdem
                FROM [DocumentosAdmissao] item
                INNER JOIN [TiposDocumento] tipo ON tipo.Id = item.TipoDocumentoId
                INNER JOIN (VALUES
                    (N'IDENTIDADE', 1),
                    (N'COMPROVANTE_RESIDENCIA', 2),
                    (N'CARTEIRA_TRABALHO', 3),
                    (N'CNH', 4),
                    (N'TITULO_ELEITOR', 5),
                    (N'COMPROVANTE_ESCOLARIDADE', 6),
                    (N'CERTIFICADO_RESERVISTA', 7)
                ) ordem(Codigo, NovaOrdem) ON ordem.Codigo = tipo.Codigo;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @CpfId uniqueidentifier = (
                    SELECT TOP(1) [Id] FROM [TiposDocumento] WHERE [Codigo] = N'CPF'
                );

                UPDATE [ConfiguracoesDocumentoAdmissao]
                SET [Ativo] = 1, [Obrigatorio] = 1, [Ordem] = 2
                WHERE [TipoDocumentoId] = @CpfId;

                UPDATE configuracao
                SET [Ordem] = ordem.OrdemAnterior
                FROM [ConfiguracoesDocumentoAdmissao] configuracao
                INNER JOIN [TiposDocumento] tipo ON tipo.Id = configuracao.TipoDocumentoId
                INNER JOIN (VALUES
                    (N'IDENTIDADE', 1),
                    (N'COMPROVANTE_RESIDENCIA', 3),
                    (N'CARTEIRA_TRABALHO', 4),
                    (N'CNH', 5),
                    (N'TITULO_ELEITOR', 6),
                    (N'COMPROVANTE_ESCOLARIDADE', 7),
                    (N'CERTIFICADO_RESERVISTA', 8)
                ) ordem(Codigo, OrdemAnterior) ON ordem.Codigo = tipo.Codigo;

                INSERT INTO [DocumentosAdmissao]
                    ([Id], [AdmissaoId], [TipoDocumentoId], [DocumentoId], [RotuloExibicao],
                     [Obrigatorio], [Ordem], [CriadoEm], [AtualizadoEm])
                SELECT NEWID(), admissao.Id, @CpfId, NULL, N'CPF', 1, 2,
                       SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM [Admissoes] admissao
                INNER JOIN [StatusAdmissao] statusAdmissao
                    ON statusAdmissao.Id = admissao.StatusAdmissaoId
                WHERE @CpfId IS NOT NULL
                  AND statusAdmissao.Codigo NOT IN (N'CONCLUIDA', N'CANCELADA')
                  AND NOT EXISTS (
                      SELECT 1 FROM [DocumentosAdmissao] item
                      WHERE item.AdmissaoId = admissao.Id
                        AND item.TipoDocumentoId = @CpfId
                  );
                """);
        }
    }
}
