using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Officio.Infra.BancoDeDados.Migrations
{
    /// <inheritdoc />
    public partial class PadronizarTabelasEmPortugues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdmissaoDependentes_Admissoes_AdmissaoId",
                table: "AdmissaoDependentes");

            migrationBuilder.DropForeignKey(
                name: "FK_AdmissaoDocumentos_Admissoes_AdmissaoId",
                table: "AdmissaoDocumentos");

            migrationBuilder.DropForeignKey(
                name: "FK_AdmissaoDocumentos_Documents_DocumentoId",
                table: "AdmissaoDocumentos");

            migrationBuilder.DropForeignKey(
                name: "FK_AdmissaoDocumentos_TiposDocumento_TipoDocumentoId",
                table: "AdmissaoDocumentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissoes_StatusAdmissoes_StatusAdmissaoId",
                table: "Admissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissoes_Tenants_EmpresaId",
                table: "Admissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturaEventos_AssinaturaSignatarios_AssinaturaSignatarioId",
                table: "AssinaturaEventos");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturaEventos_AssinaturaSolicitacoes_AssinaturaSolicitacaoId",
                table: "AssinaturaEventos");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturaSignatarios_AssinaturaSolicitacoes_AssinaturaSolicitacaoId",
                table: "AssinaturaSignatarios");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturaSignatarios_Pessoas_PessoaId",
                table: "AssinaturaSignatarios");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturaSolicitacoes_Contracts_ContratoId",
                table: "AssinaturaSolicitacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturaSolicitacoes_StatusAssinaturas_StatusAssinaturaId",
                table: "AssinaturaSolicitacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetItems_Budgets_OrcamentoId",
                table: "BudgetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Prospects_ProspectId",
                table: "Budgets");

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
                name: "FK_ConfigDocumentosAdmissao_TiposDocumento_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Budgets_BudgetId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ModelosContrato_ModeloContratoId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_OrcamentoVersoes_OrcamentoVersaoAceitaId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Prospects_ProspectId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_StatusContratosDominio_StatusContratoId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Tenants_EmpresaId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_ContratoVersoes_Contracts_ContratoId",
                table: "ContratoVersoes");

            migrationBuilder.DropForeignKey(
                name: "FK_ContratoVersoes_Documents_DocumentoId",
                table: "ContratoVersoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Admissoes_AdmissaoId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Clients_ClientId",
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
                name: "FK_DocumentValidations_Documents_DocumentId",
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
                name: "FK_EmpresaFuncionarios_Admissoes_AdmissaoOrigemId",
                table: "EmpresaFuncionarios");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpresaFuncionarios_Pessoas_PessoaId",
                table: "EmpresaFuncionarios");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpresaFuncionarios_Tenants_EmpresaId",
                table: "EmpresaFuncionarios");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpresaPessoas_Pessoas_PessoaId",
                table: "EmpresaPessoas");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpresaPessoas_Tenants_EmpresaId",
                table: "EmpresaPessoas");

            migrationBuilder.DropForeignKey(
                name: "FK_EmpresaPessoas_TiposRelacao_TipoRelacaoId",
                table: "EmpresaPessoas");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturamentosMensais_Contracts_ContratoId",
                table: "FaturamentosMensais");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturamentosMensais_StatusFaturamentos_StatusFaturamentoId",
                table: "FaturamentosMensais");

            migrationBuilder.DropForeignKey(
                name: "FK_FormularioAberturaAtividades_FormulariosAbertura_FormularioAberturaId",
                table: "FormularioAberturaAtividades");

            migrationBuilder.DropForeignKey(
                name: "FK_FormulariosAbertura_Prospects_ProspectId",
                table: "FormulariosAbertura");

            migrationBuilder.DropForeignKey(
                name: "FK_FormulariosAbertura_StatusFormularios_StatusFormularioId",
                table: "FormulariosAbertura");

            migrationBuilder.DropForeignKey(
                name: "FK_OcrCamposExtraidos_OcrExecucoes_OcrExecucaoId",
                table: "OcrCamposExtraidos");

            migrationBuilder.DropForeignKey(
                name: "FK_OcrExecucoes_Documents_DocumentoId",
                table: "OcrExecucoes");

            migrationBuilder.DropForeignKey(
                name: "FK_OrcamentoItens_BudgetCatalogItems_ItemCatalogoOrcamentoId",
                table: "OrcamentoItens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrcamentoItens_OrcamentoVersoes_OrcamentoVersaoId",
                table: "OrcamentoItens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrcamentoItens_SecoesOrcamento_SecaoOrcamentoId",
                table: "OrcamentoItens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrcamentoVersoes_Budgets_OrcamentoId",
                table: "OrcamentoVersoes");

            migrationBuilder.DropForeignKey(
                name: "FK_OrcamentoVersoes_Documents_DocumentoId",
                table: "OrcamentoVersoes");

            migrationBuilder.DropForeignKey(
                name: "FK_OrcamentoVersoes_StatusOrcamentosDominio_StatusOrcamentoId",
                table: "OrcamentoVersoes");

            migrationBuilder.DropForeignKey(
                name: "FK_PasswordResetTokens_Users_UsuarioId",
                table: "PasswordResetTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_PerfilPermissoes_PerfisUsuario_PerfilUsuarioId",
                table: "PerfilPermissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_PerfilPermissoes_Permissoes_PermissaoId",
                table: "PerfilPermissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProspectoPessoas_Pessoas_PessoaId",
                table: "ProspectoPessoas");

            migrationBuilder.DropForeignKey(
                name: "FK_ProspectoPessoas_Prospects_ProspectId",
                table: "ProspectoPessoas");

            migrationBuilder.DropForeignKey(
                name: "FK_ProspectoPessoas_TiposRelacao_TipoRelacaoId",
                table: "ProspectoPessoas");

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
                name: "FK_Signatures_Contracts_ContractId",
                table: "Signatures");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Clients_ClientId",
                table: "Socios");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Prospects_ProspectId",
                table: "Socios");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Enderecos_EnderecoId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioEmpresas_Tenants_EmpresaId",
                table: "UsuarioEmpresas");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioEmpresas_Users_UsuarioId",
                table: "UsuarioEmpresas");

            migrationBuilder.DropForeignKey(
                name: "FK_ValidacaoDivergencias_DocumentValidations_ValidacaoDocumentoId",
                table: "ValidacaoDivergencias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ValidacaoDivergencias",
                table: "ValidacaoDivergencias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuarioEmpresas",
                table: "UsuarioEmpresas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserInvitations",
                table: "UserInvitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusValidacoesDocumento",
                table: "StatusValidacoesDocumento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusProspects",
                table: "StatusProspects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusOrcamentosDominio",
                table: "StatusOrcamentosDominio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusFormularios",
                table: "StatusFormularios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusFaturamentos",
                table: "StatusFaturamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusContratosDominio",
                table: "StatusContratosDominio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusClientes",
                table: "StatusClientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusAssinaturas",
                table: "StatusAssinaturas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusAdmissoes",
                table: "StatusAdmissoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Signatures",
                table: "Signatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prospects",
                table: "Prospects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProspectoPessoas",
                table: "ProspectoPessoas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PerfilPermissoes",
                table: "PerfilPermissoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordResetTokens",
                table: "PasswordResetTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrcamentoVersoes",
                table: "OrcamentoVersoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrcamentoItens",
                table: "OrcamentoItens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OcrExecucoes",
                table: "OcrExecucoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OcrCamposExtraidos",
                table: "OcrCamposExtraidos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FormularioAberturaAtividades",
                table: "FormularioAberturaAtividades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmpresaPessoas",
                table: "EmpresaPessoas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmpresaFuncionarios",
                table: "EmpresaFuncionarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentValidations",
                table: "DocumentValidations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContratoVersoes",
                table: "ContratoVersoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contracts",
                table: "Contracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfigDocumentosAdmissao",
                table: "ConfigDocumentosAdmissao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients",
                table: "Clients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetItems",
                table: "BudgetItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetCatalogItems",
                table: "BudgetCatalogItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssinaturaSolicitacoes",
                table: "AssinaturaSolicitacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssinaturaSignatarios",
                table: "AssinaturaSignatarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssinaturaEventos",
                table: "AssinaturaEventos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdmissaoDocumentos",
                table: "AdmissaoDocumentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdmissaoDependentes",
                table: "AdmissaoDependentes");

            migrationBuilder.RenameTable(
                name: "ValidacaoDivergencias",
                newName: "DivergenciasValidacao");

            migrationBuilder.RenameTable(
                name: "UsuarioEmpresas",
                newName: "UsuariosEmpresa");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "UserInvitations",
                newName: "ConvitesUsuarios");

            migrationBuilder.RenameTable(
                name: "Tenants",
                newName: "Empresas");

            migrationBuilder.RenameTable(
                name: "StatusValidacoesDocumento",
                newName: "StatusValidacaoDocumento");

            migrationBuilder.RenameTable(
                name: "StatusProspects",
                newName: "StatusProspecto");

            migrationBuilder.RenameTable(
                name: "StatusOrcamentosDominio",
                newName: "StatusOrcamento");

            migrationBuilder.RenameTable(
                name: "StatusFormularios",
                newName: "StatusFormulario");

            migrationBuilder.RenameTable(
                name: "StatusFaturamentos",
                newName: "StatusFaturamento");

            migrationBuilder.RenameTable(
                name: "StatusContratosDominio",
                newName: "StatusContrato");

            migrationBuilder.RenameTable(
                name: "StatusClientes",
                newName: "StatusCliente");

            migrationBuilder.RenameTable(
                name: "StatusAssinaturas",
                newName: "StatusAssinatura");

            migrationBuilder.RenameTable(
                name: "StatusAdmissoes",
                newName: "StatusAdmissao");

            migrationBuilder.RenameTable(
                name: "Signatures",
                newName: "Assinaturas");

            migrationBuilder.RenameTable(
                name: "Prospects",
                newName: "Prospectos");

            migrationBuilder.RenameTable(
                name: "ProspectoPessoas",
                newName: "PessoasProspecto");

            migrationBuilder.RenameTable(
                name: "PerfilPermissoes",
                newName: "PermissoesPerfil");

            migrationBuilder.RenameTable(
                name: "PasswordResetTokens",
                newName: "TokensRedefinicaoSenha");

            migrationBuilder.RenameTable(
                name: "OrcamentoVersoes",
                newName: "VersoesOrcamento");

            migrationBuilder.RenameTable(
                name: "OrcamentoItens",
                newName: "ItensVersaoOrcamento");

            migrationBuilder.RenameTable(
                name: "OcrExecucoes",
                newName: "ExecucoesOcr");

            migrationBuilder.RenameTable(
                name: "OcrCamposExtraidos",
                newName: "CamposExtraidosOcr");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notificacoes");

            migrationBuilder.RenameTable(
                name: "FormularioAberturaAtividades",
                newName: "AtividadesFormularioAbertura");

            migrationBuilder.RenameTable(
                name: "EmpresaPessoas",
                newName: "PessoasEmpresa");

            migrationBuilder.RenameTable(
                name: "EmpresaFuncionarios",
                newName: "FuncionariosEmpresa");

            migrationBuilder.RenameTable(
                name: "DocumentValidations",
                newName: "ValidacoesDocumento");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Documentos");

            migrationBuilder.RenameTable(
                name: "ContratoVersoes",
                newName: "VersoesContrato");

            migrationBuilder.RenameTable(
                name: "Contracts",
                newName: "Contratos");

            migrationBuilder.RenameTable(
                name: "ConfigDocumentosAdmissao",
                newName: "ConfiguracoesDocumentoAdmissao");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "Clientes");

            migrationBuilder.RenameTable(
                name: "Budgets",
                newName: "Orcamentos");

            migrationBuilder.RenameTable(
                name: "BudgetItems",
                newName: "ItensOrcamento");

            migrationBuilder.RenameTable(
                name: "BudgetCatalogItems",
                newName: "ItensCatalogoOrcamento");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "RegistrosAuditoria");

            migrationBuilder.RenameTable(
                name: "AssinaturaSolicitacoes",
                newName: "SolicitacoesAssinatura");

            migrationBuilder.RenameTable(
                name: "AssinaturaSignatarios",
                newName: "SignatariosAssinatura");

            migrationBuilder.RenameTable(
                name: "AssinaturaEventos",
                newName: "EventosAssinatura");

            migrationBuilder.RenameTable(
                name: "AdmissaoDocumentos",
                newName: "DocumentosAdmissao");

            migrationBuilder.RenameTable(
                name: "AdmissaoDependentes",
                newName: "DependentesAdmissao");

            migrationBuilder.RenameIndex(
                name: "IX_ValidacaoDivergencias_ValidacaoDocumentoId",
                table: "DivergenciasValidacao",
                newName: "IX_DivergenciasValidacao_ValidacaoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioEmpresas_UsuarioId_EmpresaId",
                table: "UsuariosEmpresa",
                newName: "IX_UsuariosEmpresa_UsuarioId_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuarioEmpresas_EmpresaId",
                table: "UsuariosEmpresa",
                newName: "IX_UsuariosEmpresa_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Usuarios",
                newName: "IX_Usuarios_Email");

            migrationBuilder.RenameIndex(
                name: "IX_UserInvitations_TokenHash",
                table: "ConvitesUsuarios",
                newName: "IX_ConvitesUsuarios_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_UserInvitations_TenantId_Email",
                table: "ConvitesUsuarios",
                newName: "IX_ConvitesUsuarios_TenantId_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_EnderecoId",
                table: "Empresas",
                newName: "IX_Empresas_EnderecoId");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_Cnpj",
                table: "Empresas",
                newName: "IX_Empresas_Cnpj");

            migrationBuilder.RenameIndex(
                name: "IX_StatusValidacoesDocumento_Codigo",
                table: "StatusValidacaoDocumento",
                newName: "IX_StatusValidacaoDocumento_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusProspects_Codigo",
                table: "StatusProspecto",
                newName: "IX_StatusProspecto_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusOrcamentosDominio_Codigo",
                table: "StatusOrcamento",
                newName: "IX_StatusOrcamento_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusFormularios_Codigo",
                table: "StatusFormulario",
                newName: "IX_StatusFormulario_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusFaturamentos_Codigo",
                table: "StatusFaturamento",
                newName: "IX_StatusFaturamento_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusContratosDominio_Codigo",
                table: "StatusContrato",
                newName: "IX_StatusContrato_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusClientes_Codigo",
                table: "StatusCliente",
                newName: "IX_StatusCliente_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusAssinaturas_Codigo",
                table: "StatusAssinatura",
                newName: "IX_StatusAssinatura_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusAdmissoes_Codigo",
                table: "StatusAdmissao",
                newName: "IX_StatusAdmissao_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Signatures_ContractId",
                table: "Assinaturas",
                newName: "IX_Assinaturas_ContractId");

            migrationBuilder.RenameIndex(
                name: "IX_Prospects_UsuarioResponsavelId",
                table: "Prospectos",
                newName: "IX_Prospectos_UsuarioResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_Prospects_TenantId_Cnpj",
                table: "Prospectos",
                newName: "IX_Prospectos_TenantId_Cnpj");

            migrationBuilder.RenameIndex(
                name: "IX_Prospects_StatusProspectId",
                table: "Prospectos",
                newName: "IX_Prospectos_StatusProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_Prospects_EmpresaId",
                table: "Prospectos",
                newName: "IX_Prospectos_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_ProspectoPessoas_TipoRelacaoId",
                table: "PessoasProspecto",
                newName: "IX_PessoasProspecto_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_ProspectoPessoas_ProspectId_PessoaId_TipoRelacaoId",
                table: "PessoasProspecto",
                newName: "IX_PessoasProspecto_ProspectId_PessoaId_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_ProspectoPessoas_PessoaId",
                table: "PessoasProspecto",
                newName: "IX_PessoasProspecto_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_PerfilPermissoes_PermissaoId",
                table: "PermissoesPerfil",
                newName: "IX_PermissoesPerfil_PermissaoId");

            migrationBuilder.RenameIndex(
                name: "IX_PerfilPermissoes_PerfilUsuarioId_PermissaoId",
                table: "PermissoesPerfil",
                newName: "IX_PermissoesPerfil_PerfilUsuarioId_PermissaoId");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordResetTokens_UsuarioId",
                table: "TokensRedefinicaoSenha",
                newName: "IX_TokensRedefinicaoSenha_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordResetTokens_TokenHash",
                table: "TokensRedefinicaoSenha",
                newName: "IX_TokensRedefinicaoSenha_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoVersoes_TenantId_OrcamentoId_NumeroVersao",
                table: "VersoesOrcamento",
                newName: "IX_VersoesOrcamento_TenantId_OrcamentoId_NumeroVersao");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoVersoes_StatusOrcamentoId",
                table: "VersoesOrcamento",
                newName: "IX_VersoesOrcamento_StatusOrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoVersoes_OrcamentoId",
                table: "VersoesOrcamento",
                newName: "IX_VersoesOrcamento_OrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoVersoes_DocumentoId",
                table: "VersoesOrcamento",
                newName: "IX_VersoesOrcamento_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoItens_TenantId_OrcamentoVersaoId_Ordem",
                table: "ItensVersaoOrcamento",
                newName: "IX_ItensVersaoOrcamento_TenantId_OrcamentoVersaoId_Ordem");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoItens_SecaoOrcamentoId",
                table: "ItensVersaoOrcamento",
                newName: "IX_ItensVersaoOrcamento_SecaoOrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoItens_OrcamentoVersaoId",
                table: "ItensVersaoOrcamento",
                newName: "IX_ItensVersaoOrcamento_OrcamentoVersaoId");

            migrationBuilder.RenameIndex(
                name: "IX_OrcamentoItens_ItemCatalogoOrcamentoId",
                table: "ItensVersaoOrcamento",
                newName: "IX_ItensVersaoOrcamento_ItemCatalogoOrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_OcrExecucoes_DocumentoId",
                table: "ExecucoesOcr",
                newName: "IX_ExecucoesOcr_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_OcrCamposExtraidos_OcrExecucaoId",
                table: "CamposExtraidosOcr",
                newName: "IX_CamposExtraidosOcr_OcrExecucaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_TenantId_UserId_Tipo_ReferenciaId",
                table: "Notificacoes",
                newName: "IX_Notificacoes_TenantId_UserId_Tipo_ReferenciaId");

            migrationBuilder.RenameIndex(
                name: "IX_FormularioAberturaAtividades_FormularioAberturaId",
                table: "AtividadesFormularioAbertura",
                newName: "IX_AtividadesFormularioAbertura_FormularioAberturaId");

            migrationBuilder.RenameIndex(
                name: "IX_EmpresaPessoas_TipoRelacaoId",
                table: "PessoasEmpresa",
                newName: "IX_PessoasEmpresa_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_EmpresaPessoas_PessoaId",
                table: "PessoasEmpresa",
                newName: "IX_PessoasEmpresa_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_EmpresaPessoas_EmpresaId_PessoaId_TipoRelacaoId",
                table: "PessoasEmpresa",
                newName: "IX_PessoasEmpresa_EmpresaId_PessoaId_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_EmpresaFuncionarios_PessoaId",
                table: "FuncionariosEmpresa",
                newName: "IX_FuncionariosEmpresa_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_EmpresaFuncionarios_EmpresaId",
                table: "FuncionariosEmpresa",
                newName: "IX_FuncionariosEmpresa_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_EmpresaFuncionarios_AdmissaoOrigemId",
                table: "FuncionariosEmpresa",
                newName: "IX_FuncionariosEmpresa_AdmissaoOrigemId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentValidations_StatusValidacaoDocumentoId",
                table: "ValidacoesDocumento",
                newName: "IX_ValidacoesDocumento_StatusValidacaoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentValidations_RevisadoPorId",
                table: "ValidacoesDocumento",
                newName: "IX_ValidacoesDocumento_RevisadoPorId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentValidations_OcrExecucaoId",
                table: "ValidacoesDocumento",
                newName: "IX_ValidacoesDocumento_OcrExecucaoId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentValidations_DocumentId",
                table: "ValidacoesDocumento",
                newName: "IX_ValidacoesDocumento_DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentValidations_AdmissaoDocumentoId",
                table: "ValidacoesDocumento",
                newName: "IX_ValidacoesDocumento_AdmissaoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_TipoDocumentoId",
                table: "Documentos",
                newName: "IX_Documentos_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_EnviadoPorId",
                table: "Documentos",
                newName: "IX_Documentos_EnviadoPorId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_EmpresaId",
                table: "Documentos",
                newName: "IX_Documentos_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ContratoId",
                table: "Documentos",
                newName: "IX_Documentos_ContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ClientId",
                table: "Documentos",
                newName: "IX_Documentos_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_AdmissaoId",
                table: "Documentos",
                newName: "IX_Documentos_AdmissaoId");

            migrationBuilder.RenameIndex(
                name: "IX_ContratoVersoes_TenantId_ContratoId_NumeroVersao",
                table: "VersoesContrato",
                newName: "IX_VersoesContrato_TenantId_ContratoId_NumeroVersao");

            migrationBuilder.RenameIndex(
                name: "IX_ContratoVersoes_DocumentoId",
                table: "VersoesContrato",
                newName: "IX_VersoesContrato_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_ContratoVersoes_ContratoId",
                table: "VersoesContrato",
                newName: "IX_VersoesContrato_ContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_TenantId_Numero",
                table: "Contratos",
                newName: "IX_Contratos_TenantId_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_TenantId_BudgetId",
                table: "Contratos",
                newName: "IX_Contratos_TenantId_BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_StatusContratoId",
                table: "Contratos",
                newName: "IX_Contratos_StatusContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_ProspectId",
                table: "Contratos",
                newName: "IX_Contratos_ProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_OrcamentoVersaoAceitaId",
                table: "Contratos",
                newName: "IX_Contratos_OrcamentoVersaoAceitaId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_ModeloContratoId",
                table: "Contratos",
                newName: "IX_Contratos_ModeloContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_EmpresaId",
                table: "Contratos",
                newName: "IX_Contratos_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_ClientId",
                table: "Contratos",
                newName: "IX_Contratos_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_BudgetId",
                table: "Contratos",
                newName: "IX_Contratos_BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigDocumentosAdmissao_TipoDocumentoId",
                table: "ConfiguracoesDocumentoAdmissao",
                newName: "IX_ConfiguracoesDocumentoAdmissao_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_TenantId_SciCodigo",
                table: "Clientes",
                newName: "IX_Clientes_TenantId_SciCodigo");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_TenantId_ProspectOrigemId",
                table: "Clientes",
                newName: "IX_Clientes_TenantId_ProspectOrigemId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_TenantId_Cnpj",
                table: "Clientes",
                newName: "IX_Clientes_TenantId_Cnpj");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_StatusClienteId",
                table: "Clientes",
                newName: "IX_Clientes_StatusClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_ProspectOrigemId",
                table: "Clientes",
                newName: "IX_Clientes_ProspectOrigemId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_EmpresaId",
                table: "Clientes",
                newName: "IX_Clientes_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_TenantId_ProspectId",
                table: "Orcamentos",
                newName: "IX_Orcamentos_TenantId_ProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_ProspectId",
                table: "Orcamentos",
                newName: "IX_Orcamentos_ProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetItems_TenantId_OrcamentoId_Ordem",
                table: "ItensOrcamento",
                newName: "IX_ItensOrcamento_TenantId_OrcamentoId_Ordem");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetItems_OrcamentoId",
                table: "ItensOrcamento",
                newName: "IX_ItensOrcamento_OrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetCatalogItems_TenantId_Ativo_Secao",
                table: "ItensCatalogoOrcamento",
                newName: "IX_ItensCatalogoOrcamento_TenantId_Ativo_Secao");

            migrationBuilder.RenameIndex(
                name: "IX_AssinaturaSolicitacoes_StatusAssinaturaId",
                table: "SolicitacoesAssinatura",
                newName: "IX_SolicitacoesAssinatura_StatusAssinaturaId");

            migrationBuilder.RenameIndex(
                name: "IX_AssinaturaSolicitacoes_ContratoId",
                table: "SolicitacoesAssinatura",
                newName: "IX_SolicitacoesAssinatura_ContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_AssinaturaSignatarios_PessoaId",
                table: "SignatariosAssinatura",
                newName: "IX_SignatariosAssinatura_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_AssinaturaSignatarios_AssinaturaSolicitacaoId",
                table: "SignatariosAssinatura",
                newName: "IX_SignatariosAssinatura_AssinaturaSolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_AssinaturaEventos_AssinaturaSolicitacaoId",
                table: "EventosAssinatura",
                newName: "IX_EventosAssinatura_AssinaturaSolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_AssinaturaEventos_AssinaturaSignatarioId",
                table: "EventosAssinatura",
                newName: "IX_EventosAssinatura_AssinaturaSignatarioId");

            migrationBuilder.RenameIndex(
                name: "IX_AdmissaoDocumentos_TipoDocumentoId",
                table: "DocumentosAdmissao",
                newName: "IX_DocumentosAdmissao_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_AdmissaoDocumentos_DocumentoId",
                table: "DocumentosAdmissao",
                newName: "IX_DocumentosAdmissao_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_AdmissaoDocumentos_AdmissaoId_TipoDocumentoId",
                table: "DocumentosAdmissao",
                newName: "IX_DocumentosAdmissao_AdmissaoId_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_AdmissaoDependentes_AdmissaoId",
                table: "DependentesAdmissao",
                newName: "IX_DependentesAdmissao_AdmissaoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DivergenciasValidacao",
                table: "DivergenciasValidacao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuariosEmpresa",
                table: "UsuariosEmpresa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConvitesUsuarios",
                table: "ConvitesUsuarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Empresas",
                table: "Empresas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusValidacaoDocumento",
                table: "StatusValidacaoDocumento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusProspecto",
                table: "StatusProspecto",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusOrcamento",
                table: "StatusOrcamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusFormulario",
                table: "StatusFormulario",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusFaturamento",
                table: "StatusFaturamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusContrato",
                table: "StatusContrato",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusCliente",
                table: "StatusCliente",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusAssinatura",
                table: "StatusAssinatura",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusAdmissao",
                table: "StatusAdmissao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assinaturas",
                table: "Assinaturas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prospectos",
                table: "Prospectos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PessoasProspecto",
                table: "PessoasProspecto",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissoesPerfil",
                table: "PermissoesPerfil",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TokensRedefinicaoSenha",
                table: "TokensRedefinicaoSenha",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VersoesOrcamento",
                table: "VersoesOrcamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItensVersaoOrcamento",
                table: "ItensVersaoOrcamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExecucoesOcr",
                table: "ExecucoesOcr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CamposExtraidosOcr",
                table: "CamposExtraidosOcr",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notificacoes",
                table: "Notificacoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AtividadesFormularioAbertura",
                table: "AtividadesFormularioAbertura",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PessoasEmpresa",
                table: "PessoasEmpresa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FuncionariosEmpresa",
                table: "FuncionariosEmpresa",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ValidacoesDocumento",
                table: "ValidacoesDocumento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documentos",
                table: "Documentos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VersoesContrato",
                table: "VersoesContrato",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contratos",
                table: "Contratos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfiguracoesDocumentoAdmissao",
                table: "ConfiguracoesDocumentoAdmissao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orcamentos",
                table: "Orcamentos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItensOrcamento",
                table: "ItensOrcamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItensCatalogoOrcamento",
                table: "ItensCatalogoOrcamento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegistrosAuditoria",
                table: "RegistrosAuditoria",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SolicitacoesAssinatura",
                table: "SolicitacoesAssinatura",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignatariosAssinatura",
                table: "SignatariosAssinatura",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventosAssinatura",
                table: "EventosAssinatura",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentosAdmissao",
                table: "DocumentosAdmissao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DependentesAdmissao",
                table: "DependentesAdmissao",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Admissoes_Empresas_EmpresaId",
                table: "Admissoes",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissoes_StatusAdmissao_StatusAdmissaoId",
                table: "Admissoes",
                column: "StatusAdmissaoId",
                principalTable: "StatusAdmissao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assinaturas_Contratos_ContractId",
                table: "Assinaturas",
                column: "ContractId",
                principalTable: "Contratos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AtividadesFormularioAbertura_FormulariosAbertura_FormularioAberturaId",
                table: "AtividadesFormularioAbertura",
                column: "FormularioAberturaId",
                principalTable: "FormulariosAbertura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CamposExtraidosOcr_ExecucoesOcr_OcrExecucaoId",
                table: "CamposExtraidosOcr",
                column: "OcrExecucaoId",
                principalTable: "ExecucoesOcr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Prospectos_ProspectOrigemId",
                table: "Clientes",
                column: "ProspectOrigemId",
                principalTable: "Prospectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_StatusCliente_StatusClienteId",
                table: "Clientes",
                column: "StatusClienteId",
                principalTable: "StatusCliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracoesDocumentoAdmissao_TiposDocumento_TipoDocumentoId",
                table: "ConfiguracoesDocumentoAdmissao",
                column: "TipoDocumentoId",
                principalTable: "TiposDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Clientes_ClientId",
                table: "Contratos",
                column: "ClientId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Empresas_EmpresaId",
                table: "Contratos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_ModelosContrato_ModeloContratoId",
                table: "Contratos",
                column: "ModeloContratoId",
                principalTable: "ModelosContrato",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Orcamentos_BudgetId",
                table: "Contratos",
                column: "BudgetId",
                principalTable: "Orcamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Prospectos_ProspectId",
                table: "Contratos",
                column: "ProspectId",
                principalTable: "Prospectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_StatusContrato_StatusContratoId",
                table: "Contratos",
                column: "StatusContratoId",
                principalTable: "StatusContrato",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_VersoesOrcamento_OrcamentoVersaoAceitaId",
                table: "Contratos",
                column: "OrcamentoVersaoAceitaId",
                principalTable: "VersoesOrcamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DependentesAdmissao_Admissoes_AdmissaoId",
                table: "DependentesAdmissao",
                column: "AdmissaoId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DivergenciasValidacao_ValidacoesDocumento_ValidacaoDocumentoId",
                table: "DivergenciasValidacao",
                column: "ValidacaoDocumentoId",
                principalTable: "ValidacoesDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Admissoes_AdmissaoId",
                table: "Documentos",
                column: "AdmissaoId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Clientes_ClientId",
                table: "Documentos",
                column: "ClientId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Contratos_ContratoId",
                table: "Documentos",
                column: "ContratoId",
                principalTable: "Contratos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Empresas_EmpresaId",
                table: "Documentos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_TiposDocumento_TipoDocumentoId",
                table: "Documentos",
                column: "TipoDocumentoId",
                principalTable: "TiposDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Usuarios_EnviadoPorId",
                table: "Documentos",
                column: "EnviadoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAdmissao_Admissoes_AdmissaoId",
                table: "DocumentosAdmissao",
                column: "AdmissaoId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAdmissao_Documentos_DocumentoId",
                table: "DocumentosAdmissao",
                column: "DocumentoId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosAdmissao_TiposDocumento_TipoDocumentoId",
                table: "DocumentosAdmissao",
                column: "TipoDocumentoId",
                principalTable: "TiposDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Empresas_Enderecos_EnderecoId",
                table: "Empresas",
                column: "EnderecoId",
                principalTable: "Enderecos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosAssinatura_SignatariosAssinatura_AssinaturaSignatarioId",
                table: "EventosAssinatura",
                column: "AssinaturaSignatarioId",
                principalTable: "SignatariosAssinatura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventosAssinatura_SolicitacoesAssinatura_AssinaturaSolicitacaoId",
                table: "EventosAssinatura",
                column: "AssinaturaSolicitacaoId",
                principalTable: "SolicitacoesAssinatura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExecucoesOcr_Documentos_DocumentoId",
                table: "ExecucoesOcr",
                column: "DocumentoId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturamentosMensais_Contratos_ContratoId",
                table: "FaturamentosMensais",
                column: "ContratoId",
                principalTable: "Contratos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturamentosMensais_StatusFaturamento_StatusFaturamentoId",
                table: "FaturamentosMensais",
                column: "StatusFaturamentoId",
                principalTable: "StatusFaturamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FormulariosAbertura_Prospectos_ProspectId",
                table: "FormulariosAbertura",
                column: "ProspectId",
                principalTable: "Prospectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormulariosAbertura_StatusFormulario_StatusFormularioId",
                table: "FormulariosAbertura",
                column: "StatusFormularioId",
                principalTable: "StatusFormulario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FuncionariosEmpresa_Admissoes_AdmissaoOrigemId",
                table: "FuncionariosEmpresa",
                column: "AdmissaoOrigemId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FuncionariosEmpresa_Empresas_EmpresaId",
                table: "FuncionariosEmpresa",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FuncionariosEmpresa_Pessoas_PessoaId",
                table: "FuncionariosEmpresa",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensOrcamento_Orcamentos_OrcamentoId",
                table: "ItensOrcamento",
                column: "OrcamentoId",
                principalTable: "Orcamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensVersaoOrcamento_ItensCatalogoOrcamento_ItemCatalogoOrcamentoId",
                table: "ItensVersaoOrcamento",
                column: "ItemCatalogoOrcamentoId",
                principalTable: "ItensCatalogoOrcamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensVersaoOrcamento_SecoesOrcamento_SecaoOrcamentoId",
                table: "ItensVersaoOrcamento",
                column: "SecaoOrcamentoId",
                principalTable: "SecoesOrcamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensVersaoOrcamento_VersoesOrcamento_OrcamentoVersaoId",
                table: "ItensVersaoOrcamento",
                column: "OrcamentoVersaoId",
                principalTable: "VersoesOrcamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orcamentos_Prospectos_ProspectId",
                table: "Orcamentos",
                column: "ProspectId",
                principalTable: "Prospectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PermissoesPerfil_PerfisUsuario_PerfilUsuarioId",
                table: "PermissoesPerfil",
                column: "PerfilUsuarioId",
                principalTable: "PerfisUsuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PermissoesPerfil_Permissoes_PermissaoId",
                table: "PermissoesPerfil",
                column: "PermissaoId",
                principalTable: "Permissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PessoasEmpresa_Empresas_EmpresaId",
                table: "PessoasEmpresa",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PessoasEmpresa_Pessoas_PessoaId",
                table: "PessoasEmpresa",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PessoasEmpresa_TiposRelacao_TipoRelacaoId",
                table: "PessoasEmpresa",
                column: "TipoRelacaoId",
                principalTable: "TiposRelacao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PessoasProspecto_Pessoas_PessoaId",
                table: "PessoasProspecto",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PessoasProspecto_Prospectos_ProspectId",
                table: "PessoasProspecto",
                column: "ProspectId",
                principalTable: "Prospectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PessoasProspecto_TiposRelacao_TipoRelacaoId",
                table: "PessoasProspecto",
                column: "TipoRelacaoId",
                principalTable: "TiposRelacao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prospectos_Empresas_EmpresaId",
                table: "Prospectos",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prospectos_StatusProspecto_StatusProspectId",
                table: "Prospectos",
                column: "StatusProspectId",
                principalTable: "StatusProspecto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prospectos_Usuarios_UsuarioResponsavelId",
                table: "Prospectos",
                column: "UsuarioResponsavelId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SignatariosAssinatura_Pessoas_PessoaId",
                table: "SignatariosAssinatura",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SignatariosAssinatura_SolicitacoesAssinatura_AssinaturaSolicitacaoId",
                table: "SignatariosAssinatura",
                column: "AssinaturaSolicitacaoId",
                principalTable: "SolicitacoesAssinatura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Clientes_ClientId",
                table: "Socios",
                column: "ClientId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Prospectos_ProspectId",
                table: "Socios",
                column: "ProspectId",
                principalTable: "Prospectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitacoesAssinatura_Contratos_ContratoId",
                table: "SolicitacoesAssinatura",
                column: "ContratoId",
                principalTable: "Contratos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitacoesAssinatura_StatusAssinatura_StatusAssinaturaId",
                table: "SolicitacoesAssinatura",
                column: "StatusAssinaturaId",
                principalTable: "StatusAssinatura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TokensRedefinicaoSenha_Usuarios_UsuarioId",
                table: "TokensRedefinicaoSenha",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosEmpresa_Empresas_EmpresaId",
                table: "UsuariosEmpresa",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosEmpresa_Usuarios_UsuarioId",
                table: "UsuariosEmpresa",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ValidacoesDocumento_DocumentosAdmissao_AdmissaoDocumentoId",
                table: "ValidacoesDocumento",
                column: "AdmissaoDocumentoId",
                principalTable: "DocumentosAdmissao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ValidacoesDocumento_Documentos_DocumentId",
                table: "ValidacoesDocumento",
                column: "DocumentId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ValidacoesDocumento_ExecucoesOcr_OcrExecucaoId",
                table: "ValidacoesDocumento",
                column: "OcrExecucaoId",
                principalTable: "ExecucoesOcr",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ValidacoesDocumento_StatusValidacaoDocumento_StatusValidacaoDocumentoId",
                table: "ValidacoesDocumento",
                column: "StatusValidacaoDocumentoId",
                principalTable: "StatusValidacaoDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ValidacoesDocumento_Usuarios_RevisadoPorId",
                table: "ValidacoesDocumento",
                column: "RevisadoPorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VersoesContrato_Contratos_ContratoId",
                table: "VersoesContrato",
                column: "ContratoId",
                principalTable: "Contratos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VersoesContrato_Documentos_DocumentoId",
                table: "VersoesContrato",
                column: "DocumentoId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VersoesOrcamento_Documentos_DocumentoId",
                table: "VersoesOrcamento",
                column: "DocumentoId",
                principalTable: "Documentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VersoesOrcamento_Orcamentos_OrcamentoId",
                table: "VersoesOrcamento",
                column: "OrcamentoId",
                principalTable: "Orcamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VersoesOrcamento_StatusOrcamento_StatusOrcamentoId",
                table: "VersoesOrcamento",
                column: "StatusOrcamentoId",
                principalTable: "StatusOrcamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                UPDATE cliente
                SET EmpresaId = empresa.Id
                FROM Clientes AS cliente
                INNER JOIN Empresas AS empresa
                    ON empresa.Cnpj = REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')
                WHERE (cliente.EmpresaId IS NULL OR NOT EXISTS (
                        SELECT 1
                        FROM Empresas AS empresaVinculada
                        WHERE empresaVinculada.Id = cliente.EmpresaId
                            AND empresaVinculada.Cnpj = REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')
                    ))
                    AND cliente.Cnpj IS NOT NULL
                    AND LEN(REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')) = 14;

                INSERT INTO Empresas (
                    Id, EnderecoId, Nome, Cnpj, RazaoSocial, NomeFantasia,
                    InscricaoEstadual, InscricaoMunicipal, Telefone, Email,
                    TipoEmpresa, RegimeTributario, QuantidadeFuncionariosAtual,
                    QuantidadeProLaboresAtual, Ativo, CriadoEm, AtualizadoEm)
                SELECT
                    NEWID(), NULL, cliente.RazaoSocial,
                    REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', ''),
                    cliente.RazaoSocial, cliente.NomeFantasia,
                    NULL, NULL, cliente.Telefone, cliente.Email,
                    NULL, NULL, 0, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM Clientes AS cliente
                WHERE (cliente.EmpresaId IS NULL OR NOT EXISTS (
                        SELECT 1
                        FROM Empresas AS empresaVinculada
                        WHERE empresaVinculada.Id = cliente.EmpresaId
                            AND empresaVinculada.Cnpj = REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')
                    ))
                    AND cliente.Cnpj IS NOT NULL
                    AND LEN(REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')) = 14
                    AND NOT EXISTS (
                        SELECT 1
                        FROM Empresas AS empresa
                        WHERE empresa.Cnpj = REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')
                    );

                UPDATE cliente
                SET EmpresaId = empresa.Id
                FROM Clientes AS cliente
                INNER JOIN Empresas AS empresa
                    ON empresa.Cnpj = REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')
                WHERE (cliente.EmpresaId IS NULL OR NOT EXISTS (
                        SELECT 1
                        FROM Empresas AS empresaVinculada
                        WHERE empresaVinculada.Id = cliente.EmpresaId
                            AND empresaVinculada.Cnpj = REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')
                    ))
                    AND cliente.Cnpj IS NOT NULL
                    AND LEN(REPLACE(REPLACE(REPLACE(cliente.Cnpj, '.', ''), '/', ''), '-', '')) = 14;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admissoes_Empresas_EmpresaId",
                table: "Admissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Admissoes_StatusAdmissao_StatusAdmissaoId",
                table: "Admissoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Assinaturas_Contratos_ContractId",
                table: "Assinaturas");

            migrationBuilder.DropForeignKey(
                name: "FK_AtividadesFormularioAbertura_FormulariosAbertura_FormularioAberturaId",
                table: "AtividadesFormularioAbertura");

            migrationBuilder.DropForeignKey(
                name: "FK_CamposExtraidosOcr_ExecucoesOcr_OcrExecucaoId",
                table: "CamposExtraidosOcr");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Empresas_EmpresaId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Prospectos_ProspectOrigemId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_StatusCliente_StatusClienteId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracoesDocumentoAdmissao_TiposDocumento_TipoDocumentoId",
                table: "ConfiguracoesDocumentoAdmissao");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Clientes_ClientId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Empresas_EmpresaId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_ModelosContrato_ModeloContratoId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Orcamentos_BudgetId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Prospectos_ProspectId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_StatusContrato_StatusContratoId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_VersoesOrcamento_OrcamentoVersaoAceitaId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_DependentesAdmissao_Admissoes_AdmissaoId",
                table: "DependentesAdmissao");

            migrationBuilder.DropForeignKey(
                name: "FK_DivergenciasValidacao_ValidacoesDocumento_ValidacaoDocumentoId",
                table: "DivergenciasValidacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Admissoes_AdmissaoId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Clientes_ClientId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Contratos_ContratoId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Empresas_EmpresaId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_TiposDocumento_TipoDocumentoId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Usuarios_EnviadoPorId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAdmissao_Admissoes_AdmissaoId",
                table: "DocumentosAdmissao");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAdmissao_Documentos_DocumentoId",
                table: "DocumentosAdmissao");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosAdmissao_TiposDocumento_TipoDocumentoId",
                table: "DocumentosAdmissao");

            migrationBuilder.DropForeignKey(
                name: "FK_Empresas_Enderecos_EnderecoId",
                table: "Empresas");

            migrationBuilder.DropForeignKey(
                name: "FK_EventosAssinatura_SignatariosAssinatura_AssinaturaSignatarioId",
                table: "EventosAssinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_EventosAssinatura_SolicitacoesAssinatura_AssinaturaSolicitacaoId",
                table: "EventosAssinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_ExecucoesOcr_Documentos_DocumentoId",
                table: "ExecucoesOcr");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturamentosMensais_Contratos_ContratoId",
                table: "FaturamentosMensais");

            migrationBuilder.DropForeignKey(
                name: "FK_FaturamentosMensais_StatusFaturamento_StatusFaturamentoId",
                table: "FaturamentosMensais");

            migrationBuilder.DropForeignKey(
                name: "FK_FormulariosAbertura_Prospectos_ProspectId",
                table: "FormulariosAbertura");

            migrationBuilder.DropForeignKey(
                name: "FK_FormulariosAbertura_StatusFormulario_StatusFormularioId",
                table: "FormulariosAbertura");

            migrationBuilder.DropForeignKey(
                name: "FK_FuncionariosEmpresa_Admissoes_AdmissaoOrigemId",
                table: "FuncionariosEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_FuncionariosEmpresa_Empresas_EmpresaId",
                table: "FuncionariosEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_FuncionariosEmpresa_Pessoas_PessoaId",
                table: "FuncionariosEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensOrcamento_Orcamentos_OrcamentoId",
                table: "ItensOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensVersaoOrcamento_ItensCatalogoOrcamento_ItemCatalogoOrcamentoId",
                table: "ItensVersaoOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensVersaoOrcamento_SecoesOrcamento_SecaoOrcamentoId",
                table: "ItensVersaoOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensVersaoOrcamento_VersoesOrcamento_OrcamentoVersaoId",
                table: "ItensVersaoOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Orcamentos_Prospectos_ProspectId",
                table: "Orcamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_PermissoesPerfil_PerfisUsuario_PerfilUsuarioId",
                table: "PermissoesPerfil");

            migrationBuilder.DropForeignKey(
                name: "FK_PermissoesPerfil_Permissoes_PermissaoId",
                table: "PermissoesPerfil");

            migrationBuilder.DropForeignKey(
                name: "FK_PessoasEmpresa_Empresas_EmpresaId",
                table: "PessoasEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_PessoasEmpresa_Pessoas_PessoaId",
                table: "PessoasEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_PessoasEmpresa_TiposRelacao_TipoRelacaoId",
                table: "PessoasEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_PessoasProspecto_Pessoas_PessoaId",
                table: "PessoasProspecto");

            migrationBuilder.DropForeignKey(
                name: "FK_PessoasProspecto_Prospectos_ProspectId",
                table: "PessoasProspecto");

            migrationBuilder.DropForeignKey(
                name: "FK_PessoasProspecto_TiposRelacao_TipoRelacaoId",
                table: "PessoasProspecto");

            migrationBuilder.DropForeignKey(
                name: "FK_Prospectos_Empresas_EmpresaId",
                table: "Prospectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Prospectos_StatusProspecto_StatusProspectId",
                table: "Prospectos");

            migrationBuilder.DropForeignKey(
                name: "FK_Prospectos_Usuarios_UsuarioResponsavelId",
                table: "Prospectos");

            migrationBuilder.DropForeignKey(
                name: "FK_SignatariosAssinatura_Pessoas_PessoaId",
                table: "SignatariosAssinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_SignatariosAssinatura_SolicitacoesAssinatura_AssinaturaSolicitacaoId",
                table: "SignatariosAssinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Clientes_ClientId",
                table: "Socios");

            migrationBuilder.DropForeignKey(
                name: "FK_Socios_Prospectos_ProspectId",
                table: "Socios");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitacoesAssinatura_Contratos_ContratoId",
                table: "SolicitacoesAssinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitacoesAssinatura_StatusAssinatura_StatusAssinaturaId",
                table: "SolicitacoesAssinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_TokensRedefinicaoSenha_Usuarios_UsuarioId",
                table: "TokensRedefinicaoSenha");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosEmpresa_Empresas_EmpresaId",
                table: "UsuariosEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosEmpresa_Usuarios_UsuarioId",
                table: "UsuariosEmpresa");

            migrationBuilder.DropForeignKey(
                name: "FK_ValidacoesDocumento_DocumentosAdmissao_AdmissaoDocumentoId",
                table: "ValidacoesDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_ValidacoesDocumento_Documentos_DocumentId",
                table: "ValidacoesDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_ValidacoesDocumento_ExecucoesOcr_OcrExecucaoId",
                table: "ValidacoesDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_ValidacoesDocumento_StatusValidacaoDocumento_StatusValidacaoDocumentoId",
                table: "ValidacoesDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_ValidacoesDocumento_Usuarios_RevisadoPorId",
                table: "ValidacoesDocumento");

            migrationBuilder.DropForeignKey(
                name: "FK_VersoesContrato_Contratos_ContratoId",
                table: "VersoesContrato");

            migrationBuilder.DropForeignKey(
                name: "FK_VersoesContrato_Documentos_DocumentoId",
                table: "VersoesContrato");

            migrationBuilder.DropForeignKey(
                name: "FK_VersoesOrcamento_Documentos_DocumentoId",
                table: "VersoesOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_VersoesOrcamento_Orcamentos_OrcamentoId",
                table: "VersoesOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_VersoesOrcamento_StatusOrcamento_StatusOrcamentoId",
                table: "VersoesOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VersoesOrcamento",
                table: "VersoesOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VersoesContrato",
                table: "VersoesContrato");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ValidacoesDocumento",
                table: "ValidacoesDocumento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsuariosEmpresa",
                table: "UsuariosEmpresa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TokensRedefinicaoSenha",
                table: "TokensRedefinicaoSenha");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusValidacaoDocumento",
                table: "StatusValidacaoDocumento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusProspecto",
                table: "StatusProspecto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusOrcamento",
                table: "StatusOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusFormulario",
                table: "StatusFormulario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusFaturamento",
                table: "StatusFaturamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusContrato",
                table: "StatusContrato");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusCliente",
                table: "StatusCliente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusAssinatura",
                table: "StatusAssinatura");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusAdmissao",
                table: "StatusAdmissao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SolicitacoesAssinatura",
                table: "SolicitacoesAssinatura");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SignatariosAssinatura",
                table: "SignatariosAssinatura");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegistrosAuditoria",
                table: "RegistrosAuditoria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prospectos",
                table: "Prospectos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PessoasProspecto",
                table: "PessoasProspecto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PessoasEmpresa",
                table: "PessoasEmpresa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissoesPerfil",
                table: "PermissoesPerfil");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orcamentos",
                table: "Orcamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notificacoes",
                table: "Notificacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItensVersaoOrcamento",
                table: "ItensVersaoOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItensOrcamento",
                table: "ItensOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItensCatalogoOrcamento",
                table: "ItensCatalogoOrcamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FuncionariosEmpresa",
                table: "FuncionariosEmpresa");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExecucoesOcr",
                table: "ExecucoesOcr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventosAssinatura",
                table: "EventosAssinatura");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Empresas",
                table: "Empresas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentosAdmissao",
                table: "DocumentosAdmissao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documentos",
                table: "Documentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DivergenciasValidacao",
                table: "DivergenciasValidacao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DependentesAdmissao",
                table: "DependentesAdmissao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConvitesUsuarios",
                table: "ConvitesUsuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contratos",
                table: "Contratos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfiguracoesDocumentoAdmissao",
                table: "ConfiguracoesDocumentoAdmissao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CamposExtraidosOcr",
                table: "CamposExtraidosOcr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AtividadesFormularioAbertura",
                table: "AtividadesFormularioAbertura");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assinaturas",
                table: "Assinaturas");

            migrationBuilder.RenameTable(
                name: "VersoesOrcamento",
                newName: "OrcamentoVersoes");

            migrationBuilder.RenameTable(
                name: "VersoesContrato",
                newName: "ContratoVersoes");

            migrationBuilder.RenameTable(
                name: "ValidacoesDocumento",
                newName: "DocumentValidations");

            migrationBuilder.RenameTable(
                name: "UsuariosEmpresa",
                newName: "UsuarioEmpresas");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "TokensRedefinicaoSenha",
                newName: "PasswordResetTokens");

            migrationBuilder.RenameTable(
                name: "StatusValidacaoDocumento",
                newName: "StatusValidacoesDocumento");

            migrationBuilder.RenameTable(
                name: "StatusProspecto",
                newName: "StatusProspects");

            migrationBuilder.RenameTable(
                name: "StatusOrcamento",
                newName: "StatusOrcamentosDominio");

            migrationBuilder.RenameTable(
                name: "StatusFormulario",
                newName: "StatusFormularios");

            migrationBuilder.RenameTable(
                name: "StatusFaturamento",
                newName: "StatusFaturamentos");

            migrationBuilder.RenameTable(
                name: "StatusContrato",
                newName: "StatusContratosDominio");

            migrationBuilder.RenameTable(
                name: "StatusCliente",
                newName: "StatusClientes");

            migrationBuilder.RenameTable(
                name: "StatusAssinatura",
                newName: "StatusAssinaturas");

            migrationBuilder.RenameTable(
                name: "StatusAdmissao",
                newName: "StatusAdmissoes");

            migrationBuilder.RenameTable(
                name: "SolicitacoesAssinatura",
                newName: "AssinaturaSolicitacoes");

            migrationBuilder.RenameTable(
                name: "SignatariosAssinatura",
                newName: "AssinaturaSignatarios");

            migrationBuilder.RenameTable(
                name: "RegistrosAuditoria",
                newName: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "Prospectos",
                newName: "Prospects");

            migrationBuilder.RenameTable(
                name: "PessoasProspecto",
                newName: "ProspectoPessoas");

            migrationBuilder.RenameTable(
                name: "PessoasEmpresa",
                newName: "EmpresaPessoas");

            migrationBuilder.RenameTable(
                name: "PermissoesPerfil",
                newName: "PerfilPermissoes");

            migrationBuilder.RenameTable(
                name: "Orcamentos",
                newName: "Budgets");

            migrationBuilder.RenameTable(
                name: "Notificacoes",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "ItensVersaoOrcamento",
                newName: "OrcamentoItens");

            migrationBuilder.RenameTable(
                name: "ItensOrcamento",
                newName: "BudgetItems");

            migrationBuilder.RenameTable(
                name: "ItensCatalogoOrcamento",
                newName: "BudgetCatalogItems");

            migrationBuilder.RenameTable(
                name: "FuncionariosEmpresa",
                newName: "EmpresaFuncionarios");

            migrationBuilder.RenameTable(
                name: "ExecucoesOcr",
                newName: "OcrExecucoes");

            migrationBuilder.RenameTable(
                name: "EventosAssinatura",
                newName: "AssinaturaEventos");

            migrationBuilder.RenameTable(
                name: "Empresas",
                newName: "Tenants");

            migrationBuilder.RenameTable(
                name: "DocumentosAdmissao",
                newName: "AdmissaoDocumentos");

            migrationBuilder.RenameTable(
                name: "Documentos",
                newName: "Documents");

            migrationBuilder.RenameTable(
                name: "DivergenciasValidacao",
                newName: "ValidacaoDivergencias");

            migrationBuilder.RenameTable(
                name: "DependentesAdmissao",
                newName: "AdmissaoDependentes");

            migrationBuilder.RenameTable(
                name: "ConvitesUsuarios",
                newName: "UserInvitations");

            migrationBuilder.RenameTable(
                name: "Contratos",
                newName: "Contracts");

            migrationBuilder.RenameTable(
                name: "ConfiguracoesDocumentoAdmissao",
                newName: "ConfigDocumentosAdmissao");

            migrationBuilder.RenameTable(
                name: "Clientes",
                newName: "Clients");

            migrationBuilder.RenameTable(
                name: "CamposExtraidosOcr",
                newName: "OcrCamposExtraidos");

            migrationBuilder.RenameTable(
                name: "AtividadesFormularioAbertura",
                newName: "FormularioAberturaAtividades");

            migrationBuilder.RenameTable(
                name: "Assinaturas",
                newName: "Signatures");

            migrationBuilder.RenameIndex(
                name: "IX_VersoesOrcamento_TenantId_OrcamentoId_NumeroVersao",
                table: "OrcamentoVersoes",
                newName: "IX_OrcamentoVersoes_TenantId_OrcamentoId_NumeroVersao");

            migrationBuilder.RenameIndex(
                name: "IX_VersoesOrcamento_StatusOrcamentoId",
                table: "OrcamentoVersoes",
                newName: "IX_OrcamentoVersoes_StatusOrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_VersoesOrcamento_OrcamentoId",
                table: "OrcamentoVersoes",
                newName: "IX_OrcamentoVersoes_OrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_VersoesOrcamento_DocumentoId",
                table: "OrcamentoVersoes",
                newName: "IX_OrcamentoVersoes_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_VersoesContrato_TenantId_ContratoId_NumeroVersao",
                table: "ContratoVersoes",
                newName: "IX_ContratoVersoes_TenantId_ContratoId_NumeroVersao");

            migrationBuilder.RenameIndex(
                name: "IX_VersoesContrato_DocumentoId",
                table: "ContratoVersoes",
                newName: "IX_ContratoVersoes_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_VersoesContrato_ContratoId",
                table: "ContratoVersoes",
                newName: "IX_ContratoVersoes_ContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_ValidacoesDocumento_StatusValidacaoDocumentoId",
                table: "DocumentValidations",
                newName: "IX_DocumentValidations_StatusValidacaoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_ValidacoesDocumento_RevisadoPorId",
                table: "DocumentValidations",
                newName: "IX_DocumentValidations_RevisadoPorId");

            migrationBuilder.RenameIndex(
                name: "IX_ValidacoesDocumento_OcrExecucaoId",
                table: "DocumentValidations",
                newName: "IX_DocumentValidations_OcrExecucaoId");

            migrationBuilder.RenameIndex(
                name: "IX_ValidacoesDocumento_DocumentId",
                table: "DocumentValidations",
                newName: "IX_DocumentValidations_DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_ValidacoesDocumento_AdmissaoDocumentoId",
                table: "DocumentValidations",
                newName: "IX_DocumentValidations_AdmissaoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuariosEmpresa_UsuarioId_EmpresaId",
                table: "UsuarioEmpresas",
                newName: "IX_UsuarioEmpresas_UsuarioId_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_UsuariosEmpresa_EmpresaId",
                table: "UsuarioEmpresas",
                newName: "IX_UsuarioEmpresas_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_TokensRedefinicaoSenha_UsuarioId",
                table: "PasswordResetTokens",
                newName: "IX_PasswordResetTokens_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_TokensRedefinicaoSenha_TokenHash",
                table: "PasswordResetTokens",
                newName: "IX_PasswordResetTokens_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_StatusValidacaoDocumento_Codigo",
                table: "StatusValidacoesDocumento",
                newName: "IX_StatusValidacoesDocumento_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusProspecto_Codigo",
                table: "StatusProspects",
                newName: "IX_StatusProspects_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusOrcamento_Codigo",
                table: "StatusOrcamentosDominio",
                newName: "IX_StatusOrcamentosDominio_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusFormulario_Codigo",
                table: "StatusFormularios",
                newName: "IX_StatusFormularios_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusFaturamento_Codigo",
                table: "StatusFaturamentos",
                newName: "IX_StatusFaturamentos_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusContrato_Codigo",
                table: "StatusContratosDominio",
                newName: "IX_StatusContratosDominio_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusCliente_Codigo",
                table: "StatusClientes",
                newName: "IX_StatusClientes_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusAssinatura_Codigo",
                table: "StatusAssinaturas",
                newName: "IX_StatusAssinaturas_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_StatusAdmissao_Codigo",
                table: "StatusAdmissoes",
                newName: "IX_StatusAdmissoes_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitacoesAssinatura_StatusAssinaturaId",
                table: "AssinaturaSolicitacoes",
                newName: "IX_AssinaturaSolicitacoes_StatusAssinaturaId");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitacoesAssinatura_ContratoId",
                table: "AssinaturaSolicitacoes",
                newName: "IX_AssinaturaSolicitacoes_ContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_SignatariosAssinatura_PessoaId",
                table: "AssinaturaSignatarios",
                newName: "IX_AssinaturaSignatarios_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_SignatariosAssinatura_AssinaturaSolicitacaoId",
                table: "AssinaturaSignatarios",
                newName: "IX_AssinaturaSignatarios_AssinaturaSolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Prospectos_UsuarioResponsavelId",
                table: "Prospects",
                newName: "IX_Prospects_UsuarioResponsavelId");

            migrationBuilder.RenameIndex(
                name: "IX_Prospectos_TenantId_Cnpj",
                table: "Prospects",
                newName: "IX_Prospects_TenantId_Cnpj");

            migrationBuilder.RenameIndex(
                name: "IX_Prospectos_StatusProspectId",
                table: "Prospects",
                newName: "IX_Prospects_StatusProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_Prospectos_EmpresaId",
                table: "Prospects",
                newName: "IX_Prospects_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_PessoasProspecto_TipoRelacaoId",
                table: "ProspectoPessoas",
                newName: "IX_ProspectoPessoas_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_PessoasProspecto_ProspectId_PessoaId_TipoRelacaoId",
                table: "ProspectoPessoas",
                newName: "IX_ProspectoPessoas_ProspectId_PessoaId_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_PessoasProspecto_PessoaId",
                table: "ProspectoPessoas",
                newName: "IX_ProspectoPessoas_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_PessoasEmpresa_TipoRelacaoId",
                table: "EmpresaPessoas",
                newName: "IX_EmpresaPessoas_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_PessoasEmpresa_PessoaId",
                table: "EmpresaPessoas",
                newName: "IX_EmpresaPessoas_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_PessoasEmpresa_EmpresaId_PessoaId_TipoRelacaoId",
                table: "EmpresaPessoas",
                newName: "IX_EmpresaPessoas_EmpresaId_PessoaId_TipoRelacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_PermissoesPerfil_PermissaoId",
                table: "PerfilPermissoes",
                newName: "IX_PerfilPermissoes_PermissaoId");

            migrationBuilder.RenameIndex(
                name: "IX_PermissoesPerfil_PerfilUsuarioId_PermissaoId",
                table: "PerfilPermissoes",
                newName: "IX_PerfilPermissoes_PerfilUsuarioId_PermissaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Orcamentos_TenantId_ProspectId",
                table: "Budgets",
                newName: "IX_Budgets_TenantId_ProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_Orcamentos_ProspectId",
                table: "Budgets",
                newName: "IX_Budgets_ProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_Notificacoes_TenantId_UserId_Tipo_ReferenciaId",
                table: "Notifications",
                newName: "IX_Notifications_TenantId_UserId_Tipo_ReferenciaId");

            migrationBuilder.RenameIndex(
                name: "IX_ItensVersaoOrcamento_TenantId_OrcamentoVersaoId_Ordem",
                table: "OrcamentoItens",
                newName: "IX_OrcamentoItens_TenantId_OrcamentoVersaoId_Ordem");

            migrationBuilder.RenameIndex(
                name: "IX_ItensVersaoOrcamento_SecaoOrcamentoId",
                table: "OrcamentoItens",
                newName: "IX_OrcamentoItens_SecaoOrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItensVersaoOrcamento_OrcamentoVersaoId",
                table: "OrcamentoItens",
                newName: "IX_OrcamentoItens_OrcamentoVersaoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItensVersaoOrcamento_ItemCatalogoOrcamentoId",
                table: "OrcamentoItens",
                newName: "IX_OrcamentoItens_ItemCatalogoOrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItensOrcamento_TenantId_OrcamentoId_Ordem",
                table: "BudgetItems",
                newName: "IX_BudgetItems_TenantId_OrcamentoId_Ordem");

            migrationBuilder.RenameIndex(
                name: "IX_ItensOrcamento_OrcamentoId",
                table: "BudgetItems",
                newName: "IX_BudgetItems_OrcamentoId");

            migrationBuilder.RenameIndex(
                name: "IX_ItensCatalogoOrcamento_TenantId_Ativo_Secao",
                table: "BudgetCatalogItems",
                newName: "IX_BudgetCatalogItems_TenantId_Ativo_Secao");

            migrationBuilder.RenameIndex(
                name: "IX_FuncionariosEmpresa_PessoaId",
                table: "EmpresaFuncionarios",
                newName: "IX_EmpresaFuncionarios_PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_FuncionariosEmpresa_EmpresaId",
                table: "EmpresaFuncionarios",
                newName: "IX_EmpresaFuncionarios_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_FuncionariosEmpresa_AdmissaoOrigemId",
                table: "EmpresaFuncionarios",
                newName: "IX_EmpresaFuncionarios_AdmissaoOrigemId");

            migrationBuilder.RenameIndex(
                name: "IX_ExecucoesOcr_DocumentoId",
                table: "OcrExecucoes",
                newName: "IX_OcrExecucoes_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_EventosAssinatura_AssinaturaSolicitacaoId",
                table: "AssinaturaEventos",
                newName: "IX_AssinaturaEventos_AssinaturaSolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_EventosAssinatura_AssinaturaSignatarioId",
                table: "AssinaturaEventos",
                newName: "IX_AssinaturaEventos_AssinaturaSignatarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Empresas_EnderecoId",
                table: "Tenants",
                newName: "IX_Tenants_EnderecoId");

            migrationBuilder.RenameIndex(
                name: "IX_Empresas_Cnpj",
                table: "Tenants",
                newName: "IX_Tenants_Cnpj");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentosAdmissao_TipoDocumentoId",
                table: "AdmissaoDocumentos",
                newName: "IX_AdmissaoDocumentos_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentosAdmissao_DocumentoId",
                table: "AdmissaoDocumentos",
                newName: "IX_AdmissaoDocumentos_DocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentosAdmissao_AdmissaoId_TipoDocumentoId",
                table: "AdmissaoDocumentos",
                newName: "IX_AdmissaoDocumentos_AdmissaoId_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Documentos_TipoDocumentoId",
                table: "Documents",
                newName: "IX_Documents_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Documentos_EnviadoPorId",
                table: "Documents",
                newName: "IX_Documents_EnviadoPorId");

            migrationBuilder.RenameIndex(
                name: "IX_Documentos_EmpresaId",
                table: "Documents",
                newName: "IX_Documents_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Documentos_ContratoId",
                table: "Documents",
                newName: "IX_Documents_ContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_Documentos_ClientId",
                table: "Documents",
                newName: "IX_Documents_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Documentos_AdmissaoId",
                table: "Documents",
                newName: "IX_Documents_AdmissaoId");

            migrationBuilder.RenameIndex(
                name: "IX_DivergenciasValidacao_ValidacaoDocumentoId",
                table: "ValidacaoDivergencias",
                newName: "IX_ValidacaoDivergencias_ValidacaoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_DependentesAdmissao_AdmissaoId",
                table: "AdmissaoDependentes",
                newName: "IX_AdmissaoDependentes_AdmissaoId");

            migrationBuilder.RenameIndex(
                name: "IX_ConvitesUsuarios_TokenHash",
                table: "UserInvitations",
                newName: "IX_UserInvitations_TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_ConvitesUsuarios_TenantId_Email",
                table: "UserInvitations",
                newName: "IX_UserInvitations_TenantId_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_TenantId_Numero",
                table: "Contracts",
                newName: "IX_Contracts_TenantId_Numero");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_TenantId_BudgetId",
                table: "Contracts",
                newName: "IX_Contracts_TenantId_BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_StatusContratoId",
                table: "Contracts",
                newName: "IX_Contracts_StatusContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_ProspectId",
                table: "Contracts",
                newName: "IX_Contracts_ProspectId");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_OrcamentoVersaoAceitaId",
                table: "Contracts",
                newName: "IX_Contracts_OrcamentoVersaoAceitaId");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_ModeloContratoId",
                table: "Contracts",
                newName: "IX_Contracts_ModeloContratoId");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_EmpresaId",
                table: "Contracts",
                newName: "IX_Contracts_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_ClientId",
                table: "Contracts",
                newName: "IX_Contracts_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Contratos_BudgetId",
                table: "Contracts",
                newName: "IX_Contracts_BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfiguracoesDocumentoAdmissao_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao",
                newName: "IX_ConfigDocumentosAdmissao_TipoDocumentoId");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_TenantId_SciCodigo",
                table: "Clients",
                newName: "IX_Clients_TenantId_SciCodigo");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_TenantId_ProspectOrigemId",
                table: "Clients",
                newName: "IX_Clients_TenantId_ProspectOrigemId");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_TenantId_Cnpj",
                table: "Clients",
                newName: "IX_Clients_TenantId_Cnpj");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_StatusClienteId",
                table: "Clients",
                newName: "IX_Clients_StatusClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_ProspectOrigemId",
                table: "Clients",
                newName: "IX_Clients_ProspectOrigemId");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_EmpresaId",
                table: "Clients",
                newName: "IX_Clients_EmpresaId");

            migrationBuilder.RenameIndex(
                name: "IX_CamposExtraidosOcr_OcrExecucaoId",
                table: "OcrCamposExtraidos",
                newName: "IX_OcrCamposExtraidos_OcrExecucaoId");

            migrationBuilder.RenameIndex(
                name: "IX_AtividadesFormularioAbertura_FormularioAberturaId",
                table: "FormularioAberturaAtividades",
                newName: "IX_FormularioAberturaAtividades_FormularioAberturaId");

            migrationBuilder.RenameIndex(
                name: "IX_Assinaturas_ContractId",
                table: "Signatures",
                newName: "IX_Signatures_ContractId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrcamentoVersoes",
                table: "OrcamentoVersoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContratoVersoes",
                table: "ContratoVersoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentValidations",
                table: "DocumentValidations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsuarioEmpresas",
                table: "UsuarioEmpresas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordResetTokens",
                table: "PasswordResetTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusValidacoesDocumento",
                table: "StatusValidacoesDocumento",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusProspects",
                table: "StatusProspects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusOrcamentosDominio",
                table: "StatusOrcamentosDominio",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusFormularios",
                table: "StatusFormularios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusFaturamentos",
                table: "StatusFaturamentos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusContratosDominio",
                table: "StatusContratosDominio",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusClientes",
                table: "StatusClientes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusAssinaturas",
                table: "StatusAssinaturas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusAdmissoes",
                table: "StatusAdmissoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssinaturaSolicitacoes",
                table: "AssinaturaSolicitacoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssinaturaSignatarios",
                table: "AssinaturaSignatarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prospects",
                table: "Prospects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProspectoPessoas",
                table: "ProspectoPessoas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmpresaPessoas",
                table: "EmpresaPessoas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PerfilPermissoes",
                table: "PerfilPermissoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrcamentoItens",
                table: "OrcamentoItens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetItems",
                table: "BudgetItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetCatalogItems",
                table: "BudgetCatalogItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmpresaFuncionarios",
                table: "EmpresaFuncionarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OcrExecucoes",
                table: "OcrExecucoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssinaturaEventos",
                table: "AssinaturaEventos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdmissaoDocumentos",
                table: "AdmissaoDocumentos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ValidacaoDivergencias",
                table: "ValidacaoDivergencias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdmissaoDependentes",
                table: "AdmissaoDependentes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserInvitations",
                table: "UserInvitations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contracts",
                table: "Contracts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfigDocumentosAdmissao",
                table: "ConfigDocumentosAdmissao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients",
                table: "Clients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OcrCamposExtraidos",
                table: "OcrCamposExtraidos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FormularioAberturaAtividades",
                table: "FormularioAberturaAtividades",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Signatures",
                table: "Signatures",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdmissaoDependentes_Admissoes_AdmissaoId",
                table: "AdmissaoDependentes",
                column: "AdmissaoId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdmissaoDocumentos_Admissoes_AdmissaoId",
                table: "AdmissaoDocumentos",
                column: "AdmissaoId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdmissaoDocumentos_Documents_DocumentoId",
                table: "AdmissaoDocumentos",
                column: "DocumentoId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdmissaoDocumentos_TiposDocumento_TipoDocumentoId",
                table: "AdmissaoDocumentos",
                column: "TipoDocumentoId",
                principalTable: "TiposDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissoes_StatusAdmissoes_StatusAdmissaoId",
                table: "Admissoes",
                column: "StatusAdmissaoId",
                principalTable: "StatusAdmissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Admissoes_Tenants_EmpresaId",
                table: "Admissoes",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturaEventos_AssinaturaSignatarios_AssinaturaSignatarioId",
                table: "AssinaturaEventos",
                column: "AssinaturaSignatarioId",
                principalTable: "AssinaturaSignatarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturaEventos_AssinaturaSolicitacoes_AssinaturaSolicitacaoId",
                table: "AssinaturaEventos",
                column: "AssinaturaSolicitacaoId",
                principalTable: "AssinaturaSolicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturaSignatarios_AssinaturaSolicitacoes_AssinaturaSolicitacaoId",
                table: "AssinaturaSignatarios",
                column: "AssinaturaSolicitacaoId",
                principalTable: "AssinaturaSolicitacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturaSignatarios_Pessoas_PessoaId",
                table: "AssinaturaSignatarios",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturaSolicitacoes_Contracts_ContratoId",
                table: "AssinaturaSolicitacoes",
                column: "ContratoId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturaSolicitacoes_StatusAssinaturas_StatusAssinaturaId",
                table: "AssinaturaSolicitacoes",
                column: "StatusAssinaturaId",
                principalTable: "StatusAssinaturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetItems_Budgets_OrcamentoId",
                table: "BudgetItems",
                column: "OrcamentoId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Prospects_ProspectId",
                table: "Budgets",
                column: "ProspectId",
                principalTable: "Prospects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_ConfigDocumentosAdmissao_TiposDocumento_TipoDocumentoId",
                table: "ConfigDocumentosAdmissao",
                column: "TipoDocumentoId",
                principalTable: "TiposDocumento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Budgets_BudgetId",
                table: "Contracts",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "Clients",
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
                name: "FK_Contracts_Prospects_ProspectId",
                table: "Contracts",
                column: "ProspectId",
                principalTable: "Prospects",
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
                name: "FK_ContratoVersoes_Contracts_ContratoId",
                table: "ContratoVersoes",
                column: "ContratoId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContratoVersoes_Documents_DocumentoId",
                table: "ContratoVersoes",
                column: "DocumentoId",
                principalTable: "Documents",
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
                name: "FK_Documents_Clients_ClientId",
                table: "Documents",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

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
                name: "FK_DocumentValidations_Documents_DocumentId",
                table: "DocumentValidations",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_EmpresaFuncionarios_Admissoes_AdmissaoOrigemId",
                table: "EmpresaFuncionarios",
                column: "AdmissaoOrigemId",
                principalTable: "Admissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpresaFuncionarios_Pessoas_PessoaId",
                table: "EmpresaFuncionarios",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpresaFuncionarios_Tenants_EmpresaId",
                table: "EmpresaFuncionarios",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpresaPessoas_Pessoas_PessoaId",
                table: "EmpresaPessoas",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpresaPessoas_Tenants_EmpresaId",
                table: "EmpresaPessoas",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmpresaPessoas_TiposRelacao_TipoRelacaoId",
                table: "EmpresaPessoas",
                column: "TipoRelacaoId",
                principalTable: "TiposRelacao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturamentosMensais_Contracts_ContratoId",
                table: "FaturamentosMensais",
                column: "ContratoId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FaturamentosMensais_StatusFaturamentos_StatusFaturamentoId",
                table: "FaturamentosMensais",
                column: "StatusFaturamentoId",
                principalTable: "StatusFaturamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FormularioAberturaAtividades_FormulariosAbertura_FormularioAberturaId",
                table: "FormularioAberturaAtividades",
                column: "FormularioAberturaId",
                principalTable: "FormulariosAbertura",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormulariosAbertura_Prospects_ProspectId",
                table: "FormulariosAbertura",
                column: "ProspectId",
                principalTable: "Prospects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormulariosAbertura_StatusFormularios_StatusFormularioId",
                table: "FormulariosAbertura",
                column: "StatusFormularioId",
                principalTable: "StatusFormularios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OcrCamposExtraidos_OcrExecucoes_OcrExecucaoId",
                table: "OcrCamposExtraidos",
                column: "OcrExecucaoId",
                principalTable: "OcrExecucoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OcrExecucoes_Documents_DocumentoId",
                table: "OcrExecucoes",
                column: "DocumentoId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrcamentoItens_BudgetCatalogItems_ItemCatalogoOrcamentoId",
                table: "OrcamentoItens",
                column: "ItemCatalogoOrcamentoId",
                principalTable: "BudgetCatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrcamentoItens_OrcamentoVersoes_OrcamentoVersaoId",
                table: "OrcamentoItens",
                column: "OrcamentoVersaoId",
                principalTable: "OrcamentoVersoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrcamentoItens_SecoesOrcamento_SecaoOrcamentoId",
                table: "OrcamentoItens",
                column: "SecaoOrcamentoId",
                principalTable: "SecoesOrcamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrcamentoVersoes_Budgets_OrcamentoId",
                table: "OrcamentoVersoes",
                column: "OrcamentoId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrcamentoVersoes_Documents_DocumentoId",
                table: "OrcamentoVersoes",
                column: "DocumentoId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrcamentoVersoes_StatusOrcamentosDominio_StatusOrcamentoId",
                table: "OrcamentoVersoes",
                column: "StatusOrcamentoId",
                principalTable: "StatusOrcamentosDominio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetTokens_Users_UsuarioId",
                table: "PasswordResetTokens",
                column: "UsuarioId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PerfilPermissoes_PerfisUsuario_PerfilUsuarioId",
                table: "PerfilPermissoes",
                column: "PerfilUsuarioId",
                principalTable: "PerfisUsuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PerfilPermissoes_Permissoes_PermissaoId",
                table: "PerfilPermissoes",
                column: "PermissaoId",
                principalTable: "Permissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProspectoPessoas_Pessoas_PessoaId",
                table: "ProspectoPessoas",
                column: "PessoaId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProspectoPessoas_Prospects_ProspectId",
                table: "ProspectoPessoas",
                column: "ProspectId",
                principalTable: "Prospects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProspectoPessoas_TiposRelacao_TipoRelacaoId",
                table: "ProspectoPessoas",
                column: "TipoRelacaoId",
                principalTable: "TiposRelacao",
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
                name: "FK_Signatures_Contracts_ContractId",
                table: "Signatures",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Clients_ClientId",
                table: "Socios",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Socios_Prospects_ProspectId",
                table: "Socios",
                column: "ProspectId",
                principalTable: "Prospects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Enderecos_EnderecoId",
                table: "Tenants",
                column: "EnderecoId",
                principalTable: "Enderecos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioEmpresas_Tenants_EmpresaId",
                table: "UsuarioEmpresas",
                column: "EmpresaId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioEmpresas_Users_UsuarioId",
                table: "UsuarioEmpresas",
                column: "UsuarioId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ValidacaoDivergencias_DocumentValidations_ValidacaoDocumentoId",
                table: "ValidacaoDivergencias",
                column: "ValidacaoDocumentoId",
                principalTable: "DocumentValidations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
