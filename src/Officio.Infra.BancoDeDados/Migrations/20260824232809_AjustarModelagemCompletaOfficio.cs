using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class AjustarModelagemCompletaOfficio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Tenants",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Tenants",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AtualizadoEm",
                table: "Tenants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EnderecoId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InscricaoEstadual",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InscricaoMunicipal",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeFantasia",
                table: "Tenants",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeFuncionariosAtual",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeProLaboresAtual",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RazaoSocial",
                table: "Tenants",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegimeTributario",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoEmpresa",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConversao",
                table: "Prospects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEncerramento",
                table: "Prospects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataReuniao",
                table: "Prospects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimoContato",
                table: "Prospects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Prospects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoPerda",
                table: "Prospects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusProspectId",
                table: "Prospects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoProspect",
                table: "Prospects",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioResponsavelId",
                table: "Prospects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdmissaoDocumentoId",
                table: "DocumentValidations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacaoRevisao",
                table: "DocumentValidations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OcrExecucaoId",
                table: "DocumentValidations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevisadoEm",
                table: "DocumentValidations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevisadoPorId",
                table: "DocumentValidations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusValidacaoDocumentoId",
                table: "DocumentValidations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdmissaoId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContratoId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EnviadoPorId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origem",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "TamanhoBytes",
                table: "Documents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoDocumentoId",
                table: "Documents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModeloContratoId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrcamentoVersaoAceitaId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusContratoId",
                table: "Contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaId",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EncerradoEm",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InicioRelacionamentoEm",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusClienteId",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContratoVersoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroVersao = table.Column<int>(type: "int", nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GeradoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratoVersoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratoVersoes_Contracts_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratoVersoes_Documents_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModelosContrato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConteudoModelo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersaoAtual = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosContrato", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoIbge = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OcrExecucoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdExterno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusProcessamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModeloUtilizado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayloadRetorno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoErro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MensagemErro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IniciadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OcrExecucoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OcrExecucoes_Documents_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfisUsuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisUsuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecoesOrcamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecoesOrcamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusAdmissoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusAdmissoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusAssinaturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusAssinaturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusClientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusClientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusContratosDominio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusContratosDominio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusFaturamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusFaturamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusFormularios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusFormularios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusOrcamentosDominio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusOrcamentosDominio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusProspects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusProspects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusValidacoesDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusValidacoesDocumento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDocumento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposRelacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Escopo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposRelacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioEmpresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioEmpresas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioEmpresas_Tenants_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioEmpresas_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValidacaoDivergencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ValidacaoDocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoCampo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoDivergencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorInformado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorExtraido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confianca = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidacaoDivergencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidacaoDivergencias_DocumentValidations_ValidacaoDocumentoId",
                        column: x => x.ValidacaoDocumentoId,
                        principalTable: "DocumentValidations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enderecos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MunicipioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cep = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Logradouro = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Complemento = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enderecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enderecos_Municipios_MunicipioId",
                        column: x => x.MunicipioId,
                        principalTable: "Municipios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OcrCamposExtraidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OcrExecucaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoCampo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorExtraido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confianca = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    Pagina = table.Column<int>(type: "int", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OcrCamposExtraidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OcrCamposExtraidos_OcrExecucoes_OcrExecucaoId",
                        column: x => x.OcrExecucaoId,
                        principalTable: "OcrExecucoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilPermissoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerfilUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilPermissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilPermissoes_PerfisUsuario_PerfilUsuarioId",
                        column: x => x.PerfilUsuarioId,
                        principalTable: "PerfisUsuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilPermissoes_Permissoes_PermissaoId",
                        column: x => x.PermissaoId,
                        principalTable: "Permissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssinaturaSolicitacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusAssinaturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provedor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdExterno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SolicitadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssinaturaSolicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssinaturaSolicitacoes_Contracts_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssinaturaSolicitacoes_StatusAssinaturas_StatusAssinaturaId",
                        column: x => x.StatusAssinaturaId,
                        principalTable: "StatusAssinaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaturamentosMensais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusFaturamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    QuantidadeFuncionarios = table.Column<int>(type: "int", nullable: false),
                    QuantidadeProLabores = table.Column<int>(type: "int", nullable: false),
                    QuantidadeFolhas = table.Column<int>(type: "int", nullable: false),
                    ValorHonorarioBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorPorFolha = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturamentosMensais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturamentosMensais_Contracts_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaturamentosMensais_StatusFaturamentos_StatusFaturamentoId",
                        column: x => x.StatusFaturamentoId,
                        principalTable: "StatusFaturamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrcamentoVersoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrcamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusOrcamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroVersao = table.Column<int>(type: "int", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoEmpresaSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegimeTributarioSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuantidadeFuncionariosSnapshot = table.Column<int>(type: "int", nullable: false),
                    QuantidadeProLaboresSnapshot = table.Column<int>(type: "int", nullable: false),
                    DiaVencimento = table.Column<int>(type: "int", nullable: false),
                    ValidoAte = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnviadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AceitoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRecusa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrcamentoVersoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrcamentoVersoes_Budgets_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrcamentoVersoes_Documents_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrcamentoVersoes_StatusOrcamentosDominio_StatusOrcamentoId",
                        column: x => x.StatusOrcamentoId,
                        principalTable: "StatusOrcamentosDominio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfigDocumentosAdmissao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RotuloExibicao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Obrigatorio = table.Column<bool>(type: "bit", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigDocumentosAdmissao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigDocumentosAdmissao_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pessoas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnderecoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Rg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Sexo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCivil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Profissao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Naturalidade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TituloEleitor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReciboUltimaIrpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CartorioAssinatura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pessoas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pessoas_Enderecos_EnderecoId",
                        column: x => x.EnderecoId,
                        principalTable: "Enderecos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrcamentoItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrcamentoVersaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemCatalogoOrcamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecaoOrcamentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrcamentoItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrcamentoItens_BudgetCatalogItems_ItemCatalogoOrcamentoId",
                        column: x => x.ItemCatalogoOrcamentoId,
                        principalTable: "BudgetCatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrcamentoItens_OrcamentoVersoes_OrcamentoVersaoId",
                        column: x => x.OrcamentoVersaoId,
                        principalTable: "OrcamentoVersoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrcamentoItens_SecoesOrcamento_SecaoOrcamentoId",
                        column: x => x.SecaoOrcamentoId,
                        principalTable: "SecoesOrcamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Admissoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StatusAdmissaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeFuncionario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Sexo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCivil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Escolaridade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnderecoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Banco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Agencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoConta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataAdmissaoPrevista = table.Column<DateOnly>(type: "date", nullable: true),
                    Salario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Comissao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Funcao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContratoExperienciaDias = table.Column<int>(type: "int", nullable: true),
                    UtilizaValeTransporte = table.Column<bool>(type: "bit", nullable: true),
                    HorarioTrabalho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenAcessoHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiraEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreenchimentoFuncionarioConcluidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreenchimentoEmpresaConcluidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admissoes_Enderecos_EnderecoId",
                        column: x => x.EnderecoId,
                        principalTable: "Enderecos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissoes_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissoes_StatusAdmissoes_StatusAdmissaoId",
                        column: x => x.StatusAdmissaoId,
                        principalTable: "StatusAdmissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissoes_Tenants_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssinaturaSignatarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssinaturaSolicitacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Papel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    AssinadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssinaturaSignatarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssinaturaSignatarios_AssinaturaSolicitacoes_AssinaturaSolicitacaoId",
                        column: x => x.AssinaturaSolicitacaoId,
                        principalTable: "AssinaturaSolicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssinaturaSignatarios_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpresaPessoas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoRelacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PercentualParticipacao = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Administrador = table.Column<bool>(type: "bit", nullable: true),
                    Principal = table.Column<bool>(type: "bit", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaPessoas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaPessoas_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpresaPessoas_Tenants_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpresaPessoas_TiposRelacao_TipoRelacaoId",
                        column: x => x.TipoRelacaoId,
                        principalTable: "TiposRelacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormulariosAbertura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProspectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PessoaResponsavelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnderecoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StatusFormularioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeEmpresaPretendido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomeFantasiaPretendido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CadastroIptu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomeEdificacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProprietarioImovelNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProprietarioImovelCpfCnpj = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaTotalEdificacaoM2 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    AreaOcupadaEmpresaM2 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CapitalSocialPrevisto = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    EmailEmpresaPretendido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefoneEmpresaPretendido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroFuncionariosPrevisto = table.Column<int>(type: "int", nullable: true),
                    NumeroProLaboresPrevisto = table.Column<int>(type: "int", nullable: true),
                    LotacaoMaximaPublico = table.Column<int>(type: "int", nullable: true),
                    HorarioFuncionamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiasTrabalhadosMes = table.Column<int>(type: "int", nullable: true),
                    SituacaoImovel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroMaquinas = table.Column<int>(type: "int", nullable: true),
                    TipoMaquinas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnquadramentoPretendido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoEmpresaPretendido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegimeTributarioPretendido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulariosAbertura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulariosAbertura_Enderecos_EnderecoId",
                        column: x => x.EnderecoId,
                        principalTable: "Enderecos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormulariosAbertura_Pessoas_PessoaResponsavelId",
                        column: x => x.PessoaResponsavelId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormulariosAbertura_Prospects_ProspectId",
                        column: x => x.ProspectId,
                        principalTable: "Prospects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormulariosAbertura_StatusFormularios_StatusFormularioId",
                        column: x => x.StatusFormularioId,
                        principalTable: "StatusFormularios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProspectoPessoas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProspectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoRelacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Principal = table.Column<bool>(type: "bit", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProspectoPessoas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProspectoPessoas_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProspectoPessoas_Prospects_ProspectId",
                        column: x => x.ProspectId,
                        principalTable: "Prospects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProspectoPessoas_TiposRelacao_TipoRelacaoId",
                        column: x => x.TipoRelacaoId,
                        principalTable: "TiposRelacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdmissaoDependentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Parentesco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DependenteIr = table.Column<bool>(type: "bit", nullable: true),
                    SalarioFamilia = table.Column<bool>(type: "bit", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissaoDependentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdmissaoDependentes_Admissoes_AdmissaoId",
                        column: x => x.AdmissaoId,
                        principalTable: "Admissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdmissaoDocumentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RotuloExibicao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Obrigatorio = table.Column<bool>(type: "bit", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissaoDocumentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdmissaoDocumentos_Admissoes_AdmissaoId",
                        column: x => x.AdmissaoId,
                        principalTable: "Admissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdmissaoDocumentos_Documents_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdmissaoDocumentos_TiposDocumento_TipoDocumentoId",
                        column: x => x.TipoDocumentoId,
                        principalTable: "TiposDocumento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpresaFuncionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdmissaoOrigemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NomeFuncionario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataAdmissao = table.Column<DateOnly>(type: "date", nullable: false),
                    DataDesligamento = table.Column<DateOnly>(type: "date", nullable: true),
                    Funcao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Salario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaFuncionarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaFuncionarios_Admissoes_AdmissaoOrigemId",
                        column: x => x.AdmissaoOrigemId,
                        principalTable: "Admissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpresaFuncionarios_Pessoas_PessoaId",
                        column: x => x.PessoaId,
                        principalTable: "Pessoas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpresaFuncionarios_Tenants_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssinaturaEventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssinaturaSolicitacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssinaturaSignatarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoEvento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OcorridoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssinaturaEventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssinaturaEventos_AssinaturaSignatarios_AssinaturaSignatarioId",
                        column: x => x.AssinaturaSignatarioId,
                        principalTable: "AssinaturaSignatarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssinaturaEventos_AssinaturaSolicitacoes_AssinaturaSolicitacaoId",
                        column: x => x.AssinaturaSolicitacaoId,
                        principalTable: "AssinaturaSolicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormularioAberturaAtividades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormularioAberturaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DescricaoAtividade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cnae = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Percentual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RealizaNoEndereco = table.Column<bool>(type: "bit", nullable: true),
                    Ordem = table.Column<int>(type: "int", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormularioAberturaAtividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormularioAberturaAtividades_FormulariosAbertura_FormularioAberturaId",
                        column: x => x.FormularioAberturaId,
                        principalTable: "FormulariosAbertura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Cnpj",
                table: "Tenants",
                column: "Cnpj",
                unique: true,
                filter: "[Cnpj] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_EnderecoId",
                table: "Tenants",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_Prospects_EmpresaId",
                table: "Prospects",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prospects_StatusProspectId",
                table: "Prospects",
                column: "StatusProspectId");

            migrationBuilder.CreateIndex(
                name: "IX_Prospects_UsuarioResponsavelId",
                table: "Prospects",
                column: "UsuarioResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentValidations_AdmissaoDocumentoId",
                table: "DocumentValidations",
                column: "AdmissaoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentValidations_OcrExecucaoId",
                table: "DocumentValidations",
                column: "OcrExecucaoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentValidations_RevisadoPorId",
                table: "DocumentValidations",
                column: "RevisadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentValidations_StatusValidacaoDocumentoId",
                table: "DocumentValidations",
                column: "StatusValidacaoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AdmissaoId",
                table: "Documents",
                column: "AdmissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ContratoId",
                table: "Documents",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EmpresaId",
                table: "Documents",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EnviadoPorId",
                table: "Documents",
                column: "EnviadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TipoDocumentoId",
                table: "Documents",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_EmpresaId",
                table: "Contracts",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ModeloContratoId",
                table: "Contracts",
                column: "ModeloContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OrcamentoVersaoAceitaId",
                table: "Contracts",
                column: "OrcamentoVersaoAceitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_StatusContratoId",
                table: "Contracts",
                column: "StatusContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_EmpresaId",
                table: "Clients",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ProspectOrigemId",
                table: "Clients",
                column: "ProspectOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_StatusClienteId",
                table: "Clients",
                column: "StatusClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDependentes_AdmissaoId",
                table: "AdmissaoDependentes",
                column: "AdmissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDocumentos_AdmissaoId",
                table: "AdmissaoDocumentos",
                column: "AdmissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDocumentos_DocumentoId",
                table: "AdmissaoDocumentos",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissaoDocumentos_TipoDocumentoId",
                table: "AdmissaoDocumentos",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_EmpresaId",
                table: "Admissoes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_EnderecoId",
                table: "Admissoes",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_PessoaId",
                table: "Admissoes",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_StatusAdmissaoId",
                table: "Admissoes",
                column: "StatusAdmissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaEventos_AssinaturaSignatarioId",
                table: "AssinaturaEventos",
                column: "AssinaturaSignatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaEventos_AssinaturaSolicitacaoId",
                table: "AssinaturaEventos",
                column: "AssinaturaSolicitacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaSignatarios_AssinaturaSolicitacaoId",
                table: "AssinaturaSignatarios",
                column: "AssinaturaSolicitacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaSignatarios_PessoaId",
                table: "AssinaturaSignatarios",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaSolicitacoes_ContratoId",
                table: "AssinaturaSolicitacoes",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturaSolicitacoes_StatusAssinaturaId",
                table: "AssinaturaSolicitacoes",
                column: "StatusAssinaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigDocumentosAdmissao_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao",
                column: "TipoDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoVersoes_ContratoId",
                table: "ContratoVersoes",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoVersoes_DocumentoId",
                table: "ContratoVersoes",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoVersoes_TenantId_ContratoId_NumeroVersao",
                table: "ContratoVersoes",
                columns: new[] { "TenantId", "ContratoId", "NumeroVersao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaFuncionarios_AdmissaoOrigemId",
                table: "EmpresaFuncionarios",
                column: "AdmissaoOrigemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaFuncionarios_EmpresaId",
                table: "EmpresaFuncionarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaFuncionarios_PessoaId",
                table: "EmpresaFuncionarios",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaPessoas_EmpresaId_PessoaId_TipoRelacaoId",
                table: "EmpresaPessoas",
                columns: new[] { "EmpresaId", "PessoaId", "TipoRelacaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaPessoas_PessoaId",
                table: "EmpresaPessoas",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaPessoas_TipoRelacaoId",
                table: "EmpresaPessoas",
                column: "TipoRelacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_MunicipioId",
                table: "Enderecos",
                column: "MunicipioId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentosMensais_ContratoId",
                table: "FaturamentosMensais",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentosMensais_StatusFaturamentoId",
                table: "FaturamentosMensais",
                column: "StatusFaturamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturamentosMensais_TenantId_ContratoId_Competencia",
                table: "FaturamentosMensais",
                columns: new[] { "TenantId", "ContratoId", "Competencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormularioAberturaAtividades_FormularioAberturaId",
                table: "FormularioAberturaAtividades",
                column: "FormularioAberturaId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulariosAbertura_EnderecoId",
                table: "FormulariosAbertura",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulariosAbertura_PessoaResponsavelId",
                table: "FormulariosAbertura",
                column: "PessoaResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulariosAbertura_ProspectId",
                table: "FormulariosAbertura",
                column: "ProspectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormulariosAbertura_StatusFormularioId",
                table: "FormulariosAbertura",
                column: "StatusFormularioId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelosContrato_Codigo",
                table: "ModelosContrato",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipios_CodigoIbge",
                table: "Municipios",
                column: "CodigoIbge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OcrCamposExtraidos_OcrExecucaoId",
                table: "OcrCamposExtraidos",
                column: "OcrExecucaoId");

            migrationBuilder.CreateIndex(
                name: "IX_OcrExecucoes_DocumentoId",
                table: "OcrExecucoes",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoItens_ItemCatalogoOrcamentoId",
                table: "OrcamentoItens",
                column: "ItemCatalogoOrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoItens_OrcamentoVersaoId",
                table: "OrcamentoItens",
                column: "OrcamentoVersaoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoItens_SecaoOrcamentoId",
                table: "OrcamentoItens",
                column: "SecaoOrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoItens_TenantId_OrcamentoVersaoId_Ordem",
                table: "OrcamentoItens",
                columns: new[] { "TenantId", "OrcamentoVersaoId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoVersoes_DocumentoId",
                table: "OrcamentoVersoes",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoVersoes_OrcamentoId",
                table: "OrcamentoVersoes",
                column: "OrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoVersoes_StatusOrcamentoId",
                table: "OrcamentoVersoes",
                column: "StatusOrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoVersoes_TenantId_OrcamentoId_NumeroVersao",
                table: "OrcamentoVersoes",
                columns: new[] { "TenantId", "OrcamentoId", "NumeroVersao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilPermissoes_PerfilUsuarioId_PermissaoId",
                table: "PerfilPermissoes",
                columns: new[] { "PerfilUsuarioId", "PermissaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilPermissoes_PermissaoId",
                table: "PerfilPermissoes",
                column: "PermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfisUsuario_Codigo",
                table: "PerfisUsuario",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissoes_Codigo",
                table: "Permissoes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_Cpf",
                table: "Pessoas",
                column: "Cpf",
                unique: true,
                filter: "[Cpf] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoas_EnderecoId",
                table: "Pessoas",
                column: "EnderecoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProspectoPessoas_PessoaId",
                table: "ProspectoPessoas",
                column: "PessoaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProspectoPessoas_ProspectId_PessoaId_TipoRelacaoId",
                table: "ProspectoPessoas",
                columns: new[] { "ProspectId", "PessoaId", "TipoRelacaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProspectoPessoas_TipoRelacaoId",
                table: "ProspectoPessoas",
                column: "TipoRelacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_SecoesOrcamento_Codigo",
                table: "SecoesOrcamento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusAdmissoes_Codigo",
                table: "StatusAdmissoes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusAssinaturas_Codigo",
                table: "StatusAssinaturas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusClientes_Codigo",
                table: "StatusClientes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusContratosDominio_Codigo",
                table: "StatusContratosDominio",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusFaturamentos_Codigo",
                table: "StatusFaturamentos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusFormularios_Codigo",
                table: "StatusFormularios",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusOrcamentosDominio_Codigo",
                table: "StatusOrcamentosDominio",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusProspects_Codigo",
                table: "StatusProspects",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusValidacoesDocumento_Codigo",
                table: "StatusValidacoesDocumento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposDocumento_Codigo",
                table: "TiposDocumento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposRelacao_Codigo",
                table: "TiposRelacao",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEmpresas_EmpresaId",
                table: "UsuarioEmpresas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEmpresas_UsuarioId_EmpresaId",
                table: "UsuarioEmpresas",
                columns: new[] { "UsuarioId", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValidacaoDivergencias_ValidacaoDocumentoId",
                table: "ValidacaoDivergencias",
                column: "ValidacaoDocumentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Prospects_ProspectOrigemId",
                table: "Clients",
                column: "ProspectOrigemId",
                principalTable: "Prospects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_StatusClientes_StatusClienteId",
                table: "Clients",
                column: "StatusClienteId",
                principalTable: "StatusClientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Tenants_EmpresaId",
                table: "Clients",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ModelosContrato_ModeloContratoId",
                table: "Contracts",
                column: "ModeloContratoId",
                principalTable: "ModelosContrato",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_OrcamentoVersoes_OrcamentoVersaoAceitaId",
                table: "Contracts",
                column: "OrcamentoVersaoAceitaId",
                principalTable: "OrcamentoVersoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_StatusContratosDominio_StatusContratoId",
                table: "Contracts",
                column: "StatusContratoId",
                principalTable: "StatusContratosDominio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Tenants_EmpresaId",
                table: "Contracts",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Admissoes_AdmissaoId",
                table: "Documents",
                column: "AdmissaoId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Contracts_ContratoId",
                table: "Documents",
                column: "ContratoId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Tenants_EmpresaId",
                table: "Documents",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_TiposDocumento_TipoDocumentoId",
                table: "Documents",
                column: "TipoDocumentoId",
                principalTable: "TiposDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_EnviadoPorId",
                table: "Documents",
                column: "EnviadoPorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentValidations_AdmissaoDocumentos_AdmissaoDocumentoId",
                table: "DocumentValidations",
                column: "AdmissaoDocumentoId",
                principalTable: "AdmissaoDocumentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentValidations_OcrExecucoes_OcrExecucaoId",
                table: "DocumentValidations",
                column: "OcrExecucaoId",
                principalTable: "OcrExecucoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentValidations_StatusValidacoesDocumento_StatusValidacaoDocumentoId",
                table: "DocumentValidations",
                column: "StatusValidacaoDocumentoId",
                principalTable: "StatusValidacoesDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentValidations_Users_RevisadoPorId",
                table: "DocumentValidations",
                column: "RevisadoPorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prospects_StatusProspects_StatusProspectId",
                table: "Prospects",
                column: "StatusProspectId",
                principalTable: "StatusProspects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prospects_Tenants_EmpresaId",
                table: "Prospects",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prospects_Users_UsuarioResponsavelId",
                table: "Prospects",
                column: "UsuarioResponsavelId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Enderecos_EnderecoId",
                table: "Tenants",
                column: "EnderecoId",
                principalTable: "Enderecos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Prospects_ProspectOrigemId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_StatusClientes_StatusClienteId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Tenants_EmpresaId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ModelosContrato_ModeloContratoId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_OrcamentoVersoes_OrcamentoVersaoAceitaId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_StatusContratosDominio_StatusContratoId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Tenants_EmpresaId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Admissoes_AdmissaoId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Contracts_ContratoId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Tenants_EmpresaId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_TiposDocumento_TipoDocumentoId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_EnviadoPorId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentValidations_AdmissaoDocumentos_AdmissaoDocumentoId",
                table: "DocumentValidations");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentValidations_OcrExecucoes_OcrExecucaoId",
                table: "DocumentValidations");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentValidations_StatusValidacoesDocumento_StatusValidacaoDocumentoId",
                table: "DocumentValidations");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentValidations_Users_RevisadoPorId",
                table: "DocumentValidations");

            migrationBuilder.DropForeignKey(
                name: "FK_Prospects_StatusProspects_StatusProspectId",
                table: "Prospects");

            migrationBuilder.DropForeignKey(
                name: "FK_Prospects_Tenants_EmpresaId",
                table: "Prospects");

            migrationBuilder.DropForeignKey(
                name: "FK_Prospects_Users_UsuarioResponsavelId",
                table: "Prospects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Enderecos_EnderecoId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "AdmissaoDependentes");

            migrationBuilder.DropTable(
                name: "AdmissaoDocumentos");

            migrationBuilder.DropTable(
                name: "AssinaturaEventos");

            migrationBuilder.DropTable(
                name: "ConfigDocumentosAdmissao");

            migrationBuilder.DropTable(
                name: "ContratoVersoes");

            migrationBuilder.DropTable(
                name: "EmpresaFuncionarios");

            migrationBuilder.DropTable(
                name: "EmpresaPessoas");

            migrationBuilder.DropTable(
                name: "FaturamentosMensais");

            migrationBuilder.DropTable(
                name: "FormularioAberturaAtividades");

            migrationBuilder.DropTable(
                name: "ModelosContrato");

            migrationBuilder.DropTable(
                name: "OcrCamposExtraidos");

            migrationBuilder.DropTable(
                name: "OrcamentoItens");

            migrationBuilder.DropTable(
                name: "PerfilPermissoes");

            migrationBuilder.DropTable(
                name: "ProspectoPessoas");

            migrationBuilder.DropTable(
                name: "StatusClientes");

            migrationBuilder.DropTable(
                name: "StatusContratosDominio");

            migrationBuilder.DropTable(
                name: "StatusProspects");

            migrationBuilder.DropTable(
                name: "StatusValidacoesDocumento");

            migrationBuilder.DropTable(
                name: "UsuarioEmpresas");

            migrationBuilder.DropTable(
                name: "ValidacaoDivergencias");

            migrationBuilder.DropTable(
                name: "AssinaturaSignatarios");

            migrationBuilder.DropTable(
                name: "TiposDocumento");

            migrationBuilder.DropTable(
                name: "Admissoes");

            migrationBuilder.DropTable(
                name: "StatusFaturamentos");

            migrationBuilder.DropTable(
                name: "FormulariosAbertura");

            migrationBuilder.DropTable(
                name: "OcrExecucoes");

            migrationBuilder.DropTable(
                name: "OrcamentoVersoes");

            migrationBuilder.DropTable(
                name: "SecoesOrcamento");

            migrationBuilder.DropTable(
                name: "PerfisUsuario");

            migrationBuilder.DropTable(
                name: "Permissoes");

            migrationBuilder.DropTable(
                name: "TiposRelacao");

            migrationBuilder.DropTable(
                name: "AssinaturaSolicitacoes");

            migrationBuilder.DropTable(
                name: "StatusAdmissoes");

            migrationBuilder.DropTable(
                name: "Pessoas");

            migrationBuilder.DropTable(
                name: "StatusFormularios");

            migrationBuilder.DropTable(
                name: "StatusOrcamentosDominio");

            migrationBuilder.DropTable(
                name: "StatusAssinaturas");

            migrationBuilder.DropTable(
                name: "Enderecos");

            migrationBuilder.DropTable(
                name: "Municipios");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Cnpj",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_EnderecoId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Prospects_EmpresaId",
                table: "Prospects");

            migrationBuilder.DropIndex(
                name: "IX_Prospects_StatusProspectId",
                table: "Prospects");

            migrationBuilder.DropIndex(
                name: "IX_Prospects_UsuarioResponsavelId",
                table: "Prospects");

            migrationBuilder.DropIndex(
                name: "IX_DocumentValidations_AdmissaoDocumentoId",
                table: "DocumentValidations");

            migrationBuilder.DropIndex(
                name: "IX_DocumentValidations_OcrExecucaoId",
                table: "DocumentValidations");

            migrationBuilder.DropIndex(
                name: "IX_DocumentValidations_RevisadoPorId",
                table: "DocumentValidations");

            migrationBuilder.DropIndex(
                name: "IX_DocumentValidations_StatusValidacaoDocumentoId",
                table: "DocumentValidations");

            migrationBuilder.DropIndex(
                name: "IX_Documents_AdmissaoId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ContratoId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_EmpresaId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_EnviadoPorId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_TipoDocumentoId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_EmpresaId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ModeloContratoId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_OrcamentoVersaoAceitaId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_StatusContratoId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Clients_EmpresaId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_ProspectOrigemId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_StatusClienteId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "AtualizadoEm",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EnderecoId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "InscricaoEstadual",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "InscricaoMunicipal",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "NomeFantasia",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "QuantidadeFuncionariosAtual",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "QuantidadeProLaboresAtual",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "RazaoSocial",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "RegimeTributario",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TipoEmpresa",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DataConversao",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "DataEncerramento",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "DataReuniao",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "DataUltimoContato",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "MotivoPerda",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "StatusProspectId",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "TipoProspect",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "UsuarioResponsavelId",
                table: "Prospects");

            migrationBuilder.DropColumn(
                name: "AdmissaoDocumentoId",
                table: "DocumentValidations");

            migrationBuilder.DropColumn(
                name: "ObservacaoRevisao",
                table: "DocumentValidations");

            migrationBuilder.DropColumn(
                name: "OcrExecucaoId",
                table: "DocumentValidations");

            migrationBuilder.DropColumn(
                name: "RevisadoEm",
                table: "DocumentValidations");

            migrationBuilder.DropColumn(
                name: "RevisadoPorId",
                table: "DocumentValidations");

            migrationBuilder.DropColumn(
                name: "StatusValidacaoDocumentoId",
                table: "DocumentValidations");

            migrationBuilder.DropColumn(
                name: "AdmissaoId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ContratoId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnviadoPorId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Origem",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "TamanhoBytes",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ModeloContratoId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "OrcamentoVersaoAceitaId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "StatusContratoId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EncerradoEm",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "InicioRelacionamentoEm",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "StatusClienteId",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);
        }
    }
}
