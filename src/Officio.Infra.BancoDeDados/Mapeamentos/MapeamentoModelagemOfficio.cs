using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal static class MapeamentoModelagemOfficio
{
    public static void Configurar(ModelBuilder builder)
    {
        ConfigurarNomesTabelas(builder);
        ConfigurarDominios(builder);
        ConfigurarComercial(builder);
        ConfigurarOrcamentoContratoAssinatura(builder);
        ConfigurarAcessoDocumentosAdmissao(builder);
        ConfigurarOcrValidacaoFaturamento(builder);
    }

    private static void ConfigurarNomesTabelas(ModelBuilder builder)
    {
        builder.Entity<Admissao>().ToTable("Admissoes");
        builder.Entity<AdmissaoDependente>().ToTable("DependentesAdmissao");
        builder.Entity<AdmissaoDocumento>().ToTable("DocumentosAdmissao");
        builder.Entity<Assinatura>().ToTable("Assinaturas");
        builder.Entity<AssinaturaEvento>().ToTable("EventosAssinatura");
        builder.Entity<AssinaturaSignatario>().ToTable("SignatariosAssinatura");
        builder.Entity<AssinaturaSolicitacao>().ToTable("SolicitacoesAssinatura");
        builder.Entity<Cliente>().ToTable("Clientes");
        builder.Entity<ConfigDocumentoAdmissao>().ToTable("ConfiguracoesDocumentoAdmissao");
        builder.Entity<Contrato>().ToTable("Contratos");
        builder.Entity<ContratoVersao>().ToTable("VersoesContrato");
        builder.Entity<ConviteUsuario>().ToTable("ConvitesUsuarios");
        builder.Entity<Documento>().ToTable("Documentos");
        builder.Entity<EmpresaFuncionario>().ToTable("FuncionariosEmpresa");
        builder.Entity<EmpresaPessoa>().ToTable("PessoasEmpresa");
        builder.Entity<Endereco>().ToTable("Enderecos");
        builder.Entity<FaturamentoMensal>().ToTable("FaturamentosMensais");
        builder.Entity<FormularioAbertura>().ToTable("FormulariosAbertura");
        builder.Entity<FormularioAberturaAtividade>().ToTable("AtividadesFormularioAbertura");
        builder.Entity<ItemCatalogoOrcamento>().ToTable("ItensCatalogoOrcamento");
        builder.Entity<ItemOrcamento>().ToTable("ItensOrcamento");
        builder.Entity<ModeloContrato>().ToTable("ModelosContrato");
        builder.Entity<Municipio>().ToTable("Municipios");
        builder.Entity<Notificacao>().ToTable("Notificacoes");
        builder.Entity<OcrCampoExtraido>().ToTable("CamposExtraidosOcr");
        builder.Entity<OcrExecucao>().ToTable("ExecucoesOcr");
        builder.Entity<Orcamento>().ToTable("Orcamentos");
        builder.Entity<OrcamentoItem>().ToTable("ItensVersaoOrcamento");
        builder.Entity<OrcamentoVersao>().ToTable("VersoesOrcamento");
        builder.Entity<PerfilPermissao>().ToTable("PermissoesPerfil");
        builder.Entity<PerfilUsuario>().ToTable("PerfisUsuario");
        builder.Entity<Permissao>().ToTable("Permissoes");
        builder.Entity<Pessoa>().ToTable("Pessoas");
        builder.Entity<Prospecto>().ToTable("Prospectos");
        builder.Entity<ProspectoPessoa>().ToTable("PessoasProspecto");
        builder.Entity<RegistroAuditoria>().ToTable("RegistrosAuditoria");
        builder.Entity<SecaoOrcamento>().ToTable("SecoesOrcamento");
        builder.Entity<Socio>().ToTable("Socios");
        builder.Entity<StatusAdmissao>().ToTable("StatusAdmissao");
        builder.Entity<StatusAssinatura>().ToTable("StatusAssinatura");
        builder.Entity<StatusCliente>().ToTable("StatusCliente");
        builder.Entity<StatusContratoDominio>().ToTable("StatusContrato");
        builder.Entity<StatusFaturamento>().ToTable("StatusFaturamento");
        builder.Entity<StatusFormulario>().ToTable("StatusFormulario");
        builder.Entity<StatusOrcamentoDominio>().ToTable("StatusOrcamento");
        builder.Entity<StatusProspect>().ToTable("StatusProspecto");
        builder.Entity<StatusValidacaoDocumento>().ToTable("StatusValidacaoDocumento");
        builder.Entity<TipoDocumentoDominio>().ToTable("TiposDocumento");
        builder.Entity<TipoRelacao>().ToTable("TiposRelacao");
        builder.Entity<TokenRedefinicaoSenha>().ToTable("TokensRedefinicaoSenha");
        builder.Entity<Usuario>().ToTable("Usuarios");
        builder.Entity<UsuarioEmpresa>().ToTable("UsuariosEmpresa");
        builder.Entity<ValidacaoDocumento>().ToTable("ValidacoesDocumento");
        builder.Entity<ValidacaoDivergencia>().ToTable("DivergenciasValidacao");
    }

    private static void ConfigurarDominios(ModelBuilder builder)
    {
        foreach (var tipo in new[]
        {
            typeof(StatusProspect), typeof(StatusFormulario), typeof(StatusOrcamentoDominio),
            typeof(SecaoOrcamento), typeof(StatusContratoDominio), typeof(ModeloContrato),
            typeof(StatusAssinatura), typeof(PerfilUsuario), typeof(Permissao),
            typeof(StatusCliente), typeof(TipoDocumentoDominio), typeof(StatusAdmissao),
            typeof(StatusValidacaoDocumento), typeof(StatusFaturamento)
        })
        {
            builder.Entity(tipo).HasKey(nameof(EntidadeDominio.Id));
            builder.Entity(tipo).Property(nameof(EntidadeDominio.Codigo)).HasMaxLength(80).IsRequired();
            builder.Entity(tipo).Property(nameof(EntidadeDominio.Descricao)).HasMaxLength(250).IsRequired();
            builder.Entity(tipo).HasIndex(nameof(EntidadeDominio.Codigo)).IsUnique();
        }

        builder.Entity<TipoRelacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(80).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Escopo).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        builder.Entity<ModeloContrato>()
            .Property(e => e.ConteudoModelo)
            .HasColumnType("nvarchar(max)");
    }

    private static void ConfigurarComercial(ModelBuilder builder)
    {
        builder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoIbge).HasMaxLength(2).IsRequired();
            entity.Property(e => e.Sigla).HasMaxLength(2).IsRequired();
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.CodigoIbge).IsUnique();
            entity.HasIndex(e => e.Sigla).IsUnique();
        });

        builder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoIbge).HasMaxLength(7).IsRequired();
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.CodigoIbge).IsUnique();
            entity.HasIndex(e => new { e.EstadoId, e.Nome });
            entity.HasOne(e => e.Estado)
                .WithMany(e => e.Municipios)
                .HasForeignKey(e => e.EstadoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Prospecto>()
            .HasOne(e => e.Municipio)
            .WithMany()
            .HasForeignKey(e => e.MunicipioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Endereco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cep).HasMaxLength(8);
            entity.Property(e => e.Logradouro).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Numero).HasMaxLength(30);
            entity.Property(e => e.Complemento).HasMaxLength(150);
            entity.Property(e => e.Bairro).HasMaxLength(150);
            entity.HasOne(e => e.Municipio).WithMany().HasForeignKey(e => e.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Cpf).HasMaxLength(11);
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.Telefone).HasMaxLength(30);
            entity.HasOne(e => e.Endereco).WithMany().HasForeignKey(e => e.EnderecoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.Cpf).IsUnique().HasFilter("[Cpf] IS NOT NULL");
        });

        builder.Entity<ProspectoPessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Prospecto).WithMany(e => e.Pessoas).HasForeignKey(e => e.ProspectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Pessoa).WithMany().HasForeignKey(e => e.PessoaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TipoRelacao).WithMany().HasForeignKey(e => e.TipoRelacaoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.ProspectId, e.PessoaId, e.TipoRelacaoId }).IsUnique();
        });

        builder.Entity<FormularioAbertura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AreaTotalEdificacaoM2).HasPrecision(10, 2);
            entity.Property(e => e.AreaOcupadaEmpresaM2).HasPrecision(10, 2);
            entity.Property(e => e.CapitalSocialPrevisto).HasPrecision(15, 2);
            entity.HasOne(e => e.Prospecto).WithOne(e => e.FormularioAbertura).HasForeignKey<FormularioAbertura>(e => e.ProspectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PessoaResponsavel).WithMany().HasForeignKey(e => e.PessoaResponsavelId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Endereco).WithMany().HasForeignKey(e => e.EnderecoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.StatusFormulario).WithMany().HasForeignKey(e => e.StatusFormularioId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.ProspectId).IsUnique();
        });

        builder.Entity<FormularioAberturaAtividade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DescricaoAtividade).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.Cnae).HasMaxLength(7);
            entity.Property(e => e.Percentual).HasPrecision(5, 2);
            entity.HasOne(e => e.FormularioAbertura).WithMany(e => e.Atividades).HasForeignKey(e => e.FormularioAberturaId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmpresaPessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PercentualParticipacao).HasPrecision(5, 2);
            entity.HasOne(e => e.Empresa).WithMany(e => e.Pessoas).HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Pessoa).WithMany().HasForeignKey(e => e.PessoaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TipoRelacao).WithMany().HasForeignKey(e => e.TipoRelacaoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.EmpresaId, e.PessoaId, e.TipoRelacaoId }).IsUnique();
        });

        builder.Entity<Prospecto>(entity =>
        {
            entity.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.UsuarioResponsavel).WithMany().HasForeignKey(e => e.UsuarioResponsavelId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.StatusProspect).WithMany().HasForeignKey(e => e.StatusProspectId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.TipoProspect).HasMaxLength(40);
            entity.Property(e => e.Complemento).HasMaxLength(150);
        });

        builder.Entity<Cliente>(entity =>
        {
            entity.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne(e => e.StatusCliente).WithMany().HasForeignKey(e => e.StatusClienteId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne(e => e.ProspectOrigem).WithMany().HasForeignKey(e => e.ProspectOrigemId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarOrcamentoContratoAssinatura(ModelBuilder builder)
    {
        builder.Entity<Orcamento>(entity =>
        {
            entity.HasMany(e => e.Versoes).WithOne(e => e.Orcamento).HasForeignKey(e => e.OrcamentoId).OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(e => e.Versoes).HasField("_versoes");
        });

        builder.Entity<OrcamentoVersao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.StatusOrcamento).WithMany().HasForeignKey(e => e.StatusOrcamentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Documento).WithMany().HasForeignKey(e => e.DocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.TenantId, e.OrcamentoId, e.NumeroVersao }).IsUnique();
        });

        builder.Entity<OrcamentoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(80).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(300).IsRequired();
            entity.Property(e => e.ValorUnitario).HasPrecision(18, 2);
            entity.HasOne(e => e.OrcamentoVersao).WithMany(e => e.Itens).HasForeignKey(e => e.OrcamentoVersaoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SecaoOrcamento).WithMany().HasForeignKey(e => e.SecaoOrcamentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ItemCatalogoOrcamento).WithMany().HasForeignKey(e => e.ItemCatalogoOrcamentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.TenantId, e.OrcamentoVersaoId, e.Ordem });
        });

        builder.Entity<Contrato>(entity =>
        {
            entity.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne(e => e.OrcamentoVersaoAceita).WithMany().HasForeignKey(e => e.OrcamentoVersaoAceitaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ModeloContrato).WithMany().HasForeignKey(e => e.ModeloContratoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.StatusContratoDominio).WithMany().HasForeignKey(e => e.StatusContratoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Versoes).WithOne(e => e.Contrato).HasForeignKey(e => e.ContratoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.SolicitacoesAssinatura).WithOne(e => e.Contrato).HasForeignKey(e => e.ContratoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.FaturamentosMensais).WithOne(e => e.Contrato).HasForeignKey(e => e.ContratoId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ContratoVersao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Conteudo).HasColumnType("nvarchar(max)");
            entity.HasOne(e => e.Documento).WithMany().HasForeignKey(e => e.DocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.TenantId, e.ContratoId, e.NumeroVersao }).IsUnique();
        });

        builder.Entity<AssinaturaSolicitacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.StatusAssinatura).WithMany().HasForeignKey(e => e.StatusAssinaturaId).OnDelete(DeleteBehavior.Restrict);
            entity.Navigation(e => e.Signatarios).HasField("_signatarios");
            entity.Navigation(e => e.Eventos).HasField("_eventos");
        });

        builder.Entity<AssinaturaSignatario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.AssinaturaSolicitacao).WithMany(e => e.Signatarios).HasForeignKey(e => e.AssinaturaSolicitacaoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Pessoa).WithMany().HasForeignKey(e => e.PessoaId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssinaturaEvento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.AssinaturaSolicitacao).WithMany(e => e.Eventos).HasForeignKey(e => e.AssinaturaSolicitacaoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssinaturaSignatario).WithMany().HasForeignKey(e => e.AssinaturaSignatarioId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarAcessoDocumentosAdmissao(ModelBuilder builder)
    {
        builder.Entity<PerfilPermissao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PerfilUsuarioId, e.PermissaoId }).IsUnique();
        });

        builder.Entity<UsuarioEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UsuarioId, e.EmpresaId }).IsUnique();
        });

        builder.Entity<Documento>(entity =>
        {
            entity.HasOne(e => e.TipoDocumento).WithMany().HasForeignKey(e => e.TipoDocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Contrato).WithMany().HasForeignKey(e => e.ContratoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Admissao).WithMany().HasForeignKey(e => e.AdmissaoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.EnviadoPor).WithMany().HasForeignKey(e => e.EnviadoPorId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ConfigDocumentoAdmissao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.TipoDocumento).WithMany().HasForeignKey(e => e.TipoDocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.TipoDocumentoId).IsUnique();
        });

        builder.Entity<Admissao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Salario).HasPrecision(18, 2);
            entity.Property(e => e.TokenAcessoHash).HasMaxLength(64);
            entity.Property(e => e.TokenEmpresaAcessoHash).HasMaxLength(64);
            entity.HasIndex(e => e.TokenAcessoHash).IsUnique();
            entity.HasIndex(e => e.TokenEmpresaAcessoHash)
                .IsUnique()
                .HasFilter("[TokenEmpresaAcessoHash] IS NOT NULL");
            entity.HasOne(e => e.Empresa).WithMany(e => e.Admissoes).HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Pessoa).WithMany().HasForeignKey(e => e.PessoaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.StatusAdmissao).WithMany().HasForeignKey(e => e.StatusAdmissaoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Endereco).WithMany().HasForeignKey(e => e.EnderecoId).OnDelete(DeleteBehavior.Restrict);
            entity.Navigation(e => e.Documentos).HasField("_documentos");
            entity.Navigation(e => e.Dependentes).HasField("_dependentes");
        });

        builder.Entity<AdmissaoDocumento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Admissao).WithMany(e => e.Documentos).HasForeignKey(e => e.AdmissaoId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TipoDocumento).WithMany().HasForeignKey(e => e.TipoDocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Documento).WithMany().HasForeignKey(e => e.DocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.AdmissaoId, e.TipoDocumentoId }).IsUnique();
            entity.HasIndex(e => e.DocumentoId).IsUnique();
        });

        builder.Entity<AdmissaoDependente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Admissao).WithMany(e => e.Dependentes).HasForeignKey(e => e.AdmissaoId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmpresaFuncionario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Salario).HasPrecision(18, 2);
            entity.HasOne(e => e.Empresa).WithMany(e => e.Funcionarios).HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AdmissaoOrigem).WithMany().HasForeignKey(e => e.AdmissaoOrigemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Pessoa).WithMany().HasForeignKey(e => e.PessoaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.AdmissaoOrigemId).IsUnique();
        });
    }

    private static void ConfigurarOcrValidacaoFaturamento(ModelBuilder builder)
    {
        builder.Entity<OcrExecucao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PayloadRetorno).HasColumnType("nvarchar(max)");
            entity.HasOne(e => e.Documento).WithMany(e => e.ExecucoesOcr).HasForeignKey(e => e.DocumentoId).OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(e => e.Campos).HasField("_campos");
        });

        builder.Entity<OcrCampoExtraido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Confianca).HasPrecision(5, 4);
            entity.HasOne(e => e.OcrExecucao).WithMany(e => e.Campos).HasForeignKey(e => e.OcrExecucaoId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ValidacaoDocumento>(entity =>
        {
            entity.HasOne(e => e.AdmissaoDocumento).WithMany().HasForeignKey(e => e.AdmissaoDocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.OcrExecucao).WithMany().HasForeignKey(e => e.OcrExecucaoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.StatusValidacaoDocumento).WithMany().HasForeignKey(e => e.StatusValidacaoDocumentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.RevisadoPor).WithMany().HasForeignKey(e => e.RevisadoPorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Divergencias).WithOne(e => e.ValidacaoDocumento).HasForeignKey(e => e.ValidacaoDocumentoId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ValidacaoDivergencia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Confianca).HasPrecision(5, 4);
        });

        builder.Entity<FaturamentoMensal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorHonorarioBase).HasPrecision(18, 2);
            entity.Property(e => e.ValorPorFolha).HasPrecision(18, 2);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.StatusFaturamento).WithMany().HasForeignKey(e => e.StatusFaturamentoId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.TenantId, e.ContratoId, e.Competencia }).IsUnique();
        });
    }
}
