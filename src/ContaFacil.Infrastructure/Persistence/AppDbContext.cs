using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Domain.Common;
using ContaFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace ContaFacil.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentTenant currentTenant) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Prospect> Prospects => Set<Prospect>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentValidation> DocumentValidations => Set<DocumentValidation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Signature> Signatures => Set<Signature>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
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
        ICurrentTenant tenant) where T : AuditableEntity
    {
        builder.Entity<T>()
            .HasQueryFilter(e => e.TenantId == tenant.TenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Modified))
        {
            entry.Entity.AtualizadoEm = DateTime.UtcNow;
        }

        var events = ChangeTracker.Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(ct);

        return result;
    }
}
