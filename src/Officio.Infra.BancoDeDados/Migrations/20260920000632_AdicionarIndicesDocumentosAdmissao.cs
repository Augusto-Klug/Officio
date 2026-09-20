using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIndicesDocumentosAdmissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConfigDocumentosAdmissao_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao");

            migrationBuilder.DropIndex(
                name: "IX_AdmissaoDocumentos_AdmissaoId",
                table: "AdmissaoDocumentos");

            migrationBuilder.DropIndex(
                name: "IX_AdmissaoDocumentos_DocumentoId",
                table: "AdmissaoDocumentos");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigDocumentosAdmissao_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao",
                column: "TipoDocumentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDocumentos_AdmissaoId_TipoDocumentoId",
                table: "AdmissaoDocumentos",
                columns: new[] { "AdmissaoId", "TipoDocumentoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDocumentos_DocumentoId",
                table: "AdmissaoDocumentos",
                column: "DocumentoId",
                unique: true,
                filter: "[DocumentoId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConfigDocumentosAdmissao_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao");

            migrationBuilder.DropIndex(
                name: "IX_AdmissaoDocumentos_AdmissaoId_TipoDocumentoId",
                table: "AdmissaoDocumentos");

            migrationBuilder.DropIndex(
                name: "IX_AdmissaoDocumentos_DocumentoId",
                table: "AdmissaoDocumentos");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigDocumentosAdmissao_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDocumentos_AdmissaoId",
                table: "AdmissaoDocumentos",
                column: "AdmissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDocumentos_DocumentoId",
                table: "AdmissaoDocumentos",
                column: "DocumentoId");
        }
    }
}
