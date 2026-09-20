using Officio.Dominio.Interfaces;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Officio.Infra.BancoDeDados.Contextos;

public class ContextoAplicacao(
    DbContextOptions<ContextoAplicacao> options,
    IEmpresaAtual currentTenant,
    IPublisher publisher,
    IContextoAuditoria? contextoAuditoria = null) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Users => Set<Usuario>();
    public DbSet<Prospecto> Prospects => Set<Prospecto>();
    public DbSet<Cliente> Clients => Set<Cliente>();
    public DbSet<Orcamento> Budgets => Set<Orcamento>();
    public DbSet<Contrato> Contracts => Set<Contrato>();
    public DbSet<Documento> Documents => Set<Documento>();
    public DbSet<ValidacaoDocumento> DocumentValidations => Set<ValidacaoDocumento>();
    public DbSet<RegistroAuditoria> AuditLogs => Set<RegistroAuditoria>();
    public DbSet<Notificacao> Notifications => Set<Notificacao>();
    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Assinatura> Signatures => Set<Assinatura>();
    public DbSet<ItemOrcamento> BudgetItems => Set<ItemOrcamento>();
    public DbSet<ItemCatalogoOrcamento> BudgetCatalogItems => Set<ItemCatalogoOrcamento>();
    public DbSet<Municipio> Municipios => Set<Municipio>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<ProspectoPessoa> ProspectoPessoas => Set<ProspectoPessoa>();
    public DbSet<FormularioAbertura> FormulariosAbertura => Set<FormularioAbertura>();
    public DbSet<FormularioAberturaAtividade> FormularioAberturaAtividades => Set<FormularioAberturaAtividade>();
    public DbSet<EmpresaPessoa> EmpresaPessoas => Set<EmpresaPessoa>();
    public DbSet<StatusProspect> StatusProspects => Set<StatusProspect>();
    public DbSet<TipoRelacao> TiposRelacao => Set<TipoRelacao>();
    public DbSet<StatusFormulario> StatusFormularios => Set<StatusFormulario>();
    public DbSet<StatusOrcamentoDominio> StatusOrcamentosDominio => Set<StatusOrcamentoDominio>();
    public DbSet<SecaoOrcamento> SecoesOrcamento => Set<SecaoOrcamento>();
    public DbSet<OrcamentoVersao> OrcamentoVersoes => Set<OrcamentoVersao>();
    public DbSet<OrcamentoItem> OrcamentoItens => Set<OrcamentoItem>();
    public DbSet<StatusContratoDominio> StatusContratosDominio => Set<StatusContratoDominio>();
    public DbSet<ModeloContrato> ModelosContrato => Set<ModeloContrato>();
    public DbSet<ContratoVersao> ContratoVersoes => Set<ContratoVersao>();
    public DbSet<StatusAssinatura> StatusAssinaturas => Set<StatusAssinatura>();
    public DbSet<AssinaturaSolicitacao> AssinaturaSolicitacoes => Set<AssinaturaSolicitacao>();
    public DbSet<AssinaturaSignatario> AssinaturaSignatarios => Set<AssinaturaSignatario>();
    public DbSet<AssinaturaEvento> AssinaturaEventos => Set<AssinaturaEvento>();
    public DbSet<PerfilUsuario> PerfisUsuario => Set<PerfilUsuario>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<PerfilPermissao> PerfilPermissoes => Set<PerfilPermissao>();
    public DbSet<UsuarioEmpresa> UsuarioEmpresas => Set<UsuarioEmpresa>();
    public DbSet<StatusCliente> StatusClientes => Set<StatusCliente>();
    public DbSet<TipoDocumentoDominio> TiposDocumento => Set<TipoDocumentoDominio>();
    public DbSet<StatusAdmissao> StatusAdmissoes => Set<StatusAdmissao>();
    public DbSet<Admissao> Admissoes => Set<Admissao>();
    public DbSet<ConfigDocumentoAdmissao> ConfigDocumentosAdmissao => Set<ConfigDocumentoAdmissao>();
    public DbSet<AdmissaoDocumento> AdmissaoDocumentos => Set<AdmissaoDocumento>();
    public DbSet<AdmissaoDependente> AdmissaoDependentes => Set<AdmissaoDependente>();
    public DbSet<EmpresaFuncionario> EmpresaFuncionarios => Set<EmpresaFuncionario>();
    public DbSet<OcrExecucao> OcrExecucoes => Set<OcrExecucao>();
    public DbSet<OcrCampoExtraido> OcrCamposExtraidos => Set<OcrCampoExtraido>();
    public DbSet<StatusValidacaoDocumento> StatusValidacoesDocumento => Set<StatusValidacaoDocumento>();
    public DbSet<ValidacaoDivergencia> ValidacaoDivergencias => Set<ValidacaoDivergencia>();
    public DbSet<StatusFaturamento> StatusFaturamentos => Set<StatusFaturamento>();
    public DbSet<FaturamentoMensal> FaturamentosMensais => Set<FaturamentoMensal>();
    public DbSet<ConviteUsuario> ConvitesUsuario => Set<ConviteUsuario>();
    public DbSet<TokenRedefinicaoSenha> TokensRedefinicaoSenha => Set<TokenRedefinicaoSenha>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Usuario>()
            .Property(usuario => usuario.Email)
            .HasMaxLength(450);

        builder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Email)
            .IsUnique();

        builder.Entity<Usuario>(entity =>
        {
            entity.Property(usuario => usuario.Nome).HasMaxLength(200).IsRequired();
            entity.Property(usuario => usuario.SenhaHash).HasMaxLength(500).IsRequired();
            entity.Property(usuario => usuario.Role).HasMaxLength(50).IsRequired();
        });

        builder.Entity<ConviteUsuario>(entity =>
        {
            entity.ToTable("UserInvitations");
            entity.Property(convite => convite.Nome).HasMaxLength(200).IsRequired();
            entity.Property(convite => convite.Email).HasMaxLength(450).IsRequired();
            entity.Property(convite => convite.Perfil).HasMaxLength(50).IsRequired();
            entity.Property(convite => convite.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(convite => convite.TokenHash).IsUnique();
            entity.HasIndex(convite => new { convite.TenantId, convite.Email });
        });

        builder.Entity<TokenRedefinicaoSenha>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
            entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => token.UsuarioId);
            entity.HasOne(token => token.Usuario)
                .WithMany()
                .HasForeignKey(token => token.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.ApplyConfigurationsFromAssembly(typeof(ContextoAplicacao).Assembly);
        Mapeamentos.MapeamentoModelagemOfficio.Configurar(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(EntidadeAuditavel).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ContextoAplicacao)
                    .GetMethod(nameof(ApplyTenantFilter),
                        BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(this, [builder]);
            }
        }

        base.OnModelCreating(builder);
    }

    private Guid TenantIdAtual => currentTenant.TenantId;

    private void ApplyTenantFilter<T>(ModelBuilder builder) where T : EntidadeAuditavel
    {
        builder.Entity<T>()
            .HasQueryFilter(e => e.TenantId == TenantIdAtual);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var entidadesAlteradas = ChangeTracker.Entries<EntidadeAuditavel>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entidadesAlteradas.Where(entry => entry.State is EntityState.Modified))
        {
            entry.Entity.AtualizadoEm = DateTime.UtcNow;
        }

        AdicionarRegistrosAuditoria(entidadesAlteradas);

        var events = ChangeTracker.Entries<EntidadeBase>()
            .SelectMany(e => e.Entity.EventosDominio)
            .ToList();

        var result = await base.SaveChangesAsync(ct);

        foreach (var entry in ChangeTracker.Entries<EntidadeBase>())
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, ct);

        return result;
    }

    private void AdicionarRegistrosAuditoria(
        IReadOnlyCollection<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<EntidadeAuditavel>> entidadesAlteradas)
    {
        if (string.IsNullOrWhiteSpace(contextoAuditoria?.OperacaoAtual))
            return;

        var registros = entidadesAlteradas.Select(entry => RegistroAuditoria.Criar(
            entry.Entity.TenantId == Guid.Empty ? currentTenant.TenantId : entry.Entity.TenantId,
            contextoAuditoria.UsuarioIdAtual,
            entry.Metadata.ClrType.Name,
            entry.Entity.Id,
            $"{contextoAuditoria.OperacaoAtual}:{ObterAcao(entry.State)}"));

        AuditLogs.AddRange(registros);
    }

    private static string ObterAcao(EntityState estado) => estado switch
    {
        EntityState.Added => "CRIADO",
        EntityState.Modified => "ALTERADO",
        EntityState.Deleted => "REMOVIDO",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, null)
    };
}





