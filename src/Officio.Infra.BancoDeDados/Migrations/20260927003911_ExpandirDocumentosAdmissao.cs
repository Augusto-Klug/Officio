using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class ExpandirDocumentosAdmissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @Tipos TABLE (
                    Id uniqueidentifier NOT NULL,
                    Codigo nvarchar(80) NOT NULL,
                    Descricao nvarchar(250) NOT NULL,
                    Rotulo nvarchar(250) NOT NULL,
                    Obrigatorio bit NOT NULL,
                    Ordem int NOT NULL
                );

                INSERT INTO @Tipos (Id, Codigo, Descricao, Rotulo, Obrigatorio, Ordem)
                VALUES
                    ('ad000001-0000-0000-0000-000000000001', N'IDENTIDADE', N'Documento de identidade', N'Identidade / RG', 1, 1),
                    ('ad000001-0000-0000-0000-000000000002', N'CPF', N'Cadastro de Pessoa Fisica', N'CPF', 1, 2),
                    ('ad000001-0000-0000-0000-000000000003', N'COMPROVANTE_RESIDENCIA', N'Comprovante de residencia', N'Comprovante de residência', 1, 3),
                    ('ad000001-0000-0000-0000-000000000004', N'CARTEIRA_TRABALHO', N'Carteira de trabalho', N'Carteira de trabalho', 1, 4),
                    ('ad000001-0000-0000-0000-000000000005', N'CNH', N'Carteira Nacional de Habilitacao', N'Carteira de motorista / CNH', 0, 5),
                    ('ad000001-0000-0000-0000-000000000006', N'TITULO_ELEITOR', N'Titulo de eleitor', N'Título de eleitor', 0, 6),
                    ('ad000001-0000-0000-0000-000000000007', N'COMPROVANTE_ESCOLARIDADE', N'Comprovante de escolaridade', N'Comprovante de escolaridade', 0, 7),
                    ('ad000001-0000-0000-0000-000000000008', N'CERTIFICADO_RESERVISTA', N'Certificado de reservista', N'Certificado de reservista', 0, 8);

                INSERT INTO [TiposDocumento] ([Id], [Codigo], [Descricao], [Ordem], [Ativo])
                SELECT tipo.Id, tipo.Codigo, tipo.Descricao, tipo.Ordem, 1
                FROM @Tipos tipo
                WHERE NOT EXISTS (
                    SELECT 1 FROM [TiposDocumento] existente WHERE existente.Codigo = tipo.Codigo
                );

                INSERT INTO [ConfiguracoesDocumentoAdmissao]
                    ([Id], [TipoDocumentoId], [RotuloExibicao], [Obrigatorio], [Ordem], [Ativo])
                SELECT NEWID(), documento.Id, tipo.Rotulo, tipo.Obrigatorio, tipo.Ordem, 1
                FROM @Tipos tipo
                INNER JOIN [TiposDocumento] documento ON documento.Codigo = tipo.Codigo
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [ConfiguracoesDocumentoAdmissao] configuracao
                    WHERE configuracao.TipoDocumentoId = documento.Id
                );

                INSERT INTO [DocumentosAdmissao]
                    ([Id], [AdmissaoId], [TipoDocumentoId], [DocumentoId], [RotuloExibicao],
                     [Obrigatorio], [Ordem], [CriadoEm], [AtualizadoEm])
                SELECT NEWID(), admissao.Id, documento.Id, NULL, tipo.Rotulo,
                       tipo.Obrigatorio, tipo.Ordem, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM [Admissoes] admissao
                INNER JOIN [StatusAdmissao] statusAdmissao
                    ON statusAdmissao.Id = admissao.StatusAdmissaoId
                CROSS JOIN @Tipos tipo
                INNER JOIN [TiposDocumento] documento ON documento.Codigo = tipo.Codigo
                WHERE statusAdmissao.Codigo NOT IN (N'CONCLUIDA', N'CANCELADA')
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [DocumentosAdmissao] item
                      WHERE item.AdmissaoId = admissao.Id
                        AND item.TipoDocumentoId = documento.Id
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE item
                FROM [DocumentosAdmissao] item
                INNER JOIN [TiposDocumento] tipo ON tipo.Id = item.TipoDocumentoId
                WHERE tipo.Codigo IN (
                    N'CNH', N'TITULO_ELEITOR', N'COMPROVANTE_ESCOLARIDADE', N'CERTIFICADO_RESERVISTA'
                ) AND item.DocumentoId IS NULL;

                DELETE configuracao
                FROM [ConfiguracoesDocumentoAdmissao] configuracao
                INNER JOIN [TiposDocumento] tipo ON tipo.Id = configuracao.TipoDocumentoId
                WHERE tipo.Codigo IN (
                    N'CNH', N'TITULO_ELEITOR', N'COMPROVANTE_ESCOLARIDADE', N'CERTIFICADO_RESERVISTA'
                );

                DELETE tipo
                FROM [TiposDocumento] tipo
                WHERE tipo.Codigo IN (
                    N'CNH', N'TITULO_ELEITOR', N'COMPROVANTE_ESCOLARIDADE', N'CERTIFICADO_RESERVISTA'
                )
                  AND NOT EXISTS (
                      SELECT 1 FROM [DocumentosAdmissao] item WHERE item.TipoDocumentoId = tipo.Id
                  )
                  AND NOT EXISTS (
                      SELECT 1 FROM [Documentos] documento WHERE documento.TipoDocumentoId = tipo.Id
                  );
                """);
        }
    }
}
