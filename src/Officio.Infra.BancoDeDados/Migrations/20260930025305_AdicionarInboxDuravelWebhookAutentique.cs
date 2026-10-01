using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarInboxDuravelWebhookAutentique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutentiqueWebhookInbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HashPayload = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceParent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TraceState = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RecebidoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProximaTentativaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BloqueadoAte = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tentativas = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UltimoErro = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutentiqueWebhookInbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutentiqueWebhookInbox_HashPayload",
                table: "AutentiqueWebhookInbox",
                column: "HashPayload",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutentiqueWebhookInbox_Status_ProximaTentativaEm_BloqueadoAte_RecebidoEm",
                table: "AutentiqueWebhookInbox",
                columns: new[] { "Status", "ProximaTentativaEm", "BloqueadoAte", "RecebidoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutentiqueWebhookInbox");
        }
    }
}
