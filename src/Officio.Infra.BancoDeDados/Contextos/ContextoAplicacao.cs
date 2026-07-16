using Officio.Dominio.Interfaces;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace Officio.Infra.BancoDeDados.Contextos;

public class ContextoAplicacao(
    DbContextOptions<ContextoAplicacao> options,
    IEmpresaAtual currentTenant) : DbContext(options)
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ContextoAplicacao).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(EntidadeAuditavel).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ContextoAplicacao)
                    .GetMethod(nameof(ApplyTenantFilter),
                        BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, [builder, currentTenant]);
            }
        }

        base.OnModelCreating(builder);
    }

    private static void ApplyTenantFilter<T>(
        ModelBuilder builder,
        IEmpresaAtual tenant) where T : EntidadeAuditavel
    {
        builder.Entity<T>()
            .HasQueryFilter(e => e.TenantId == tenant.TenantId);
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

        return result;
    }
}





