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
    IPublisher publisher) : DbContext(options)
{
    public DbSet<Empresa> Tenants => Set<Empresa>();
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Usuario>()
            .Property(usuario => usuario.Email)
            .HasMaxLength(450);

        builder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Email)
            .IsUnique();

        builder.ApplyConfigurationsFromAssembly(typeof(ContextoAplicacao).Assembly);

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
        foreach (var entry in ChangeTracker.Entries<EntidadeAuditavel>()
            .Where(e => e.State is EntityState.Modified))
        {
            entry.Entity.AtualizadoEm = DateTime.UtcNow;
        }

        var events = ChangeTracker.Entries<EntidadeBase>()
            .SelectMany(e => e.Entity.EventosDominio)
            .ToList();

        var result = await base.SaveChangesAsync(ct);

        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, ct);

        foreach (var entry in ChangeTracker.Entries<EntidadeBase>())
            entry.Entity.ClearDomainEvents();

        return result;
    }
}





