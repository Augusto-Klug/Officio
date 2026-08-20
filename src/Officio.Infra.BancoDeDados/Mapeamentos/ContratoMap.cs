using Officio.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal class ContratoMap : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Numero)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.ValorTotal)
            .HasPrecision(18, 2);

        builder.Property(c => c.ClientId)
            .IsRequired(false);

        builder.HasOne(c => c.Prospecto)
            .WithMany()
            .HasForeignKey(c => c.ProspectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Cliente)
            .WithMany(c => c.Contratos)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(c => c.Orcamento)
            .WithOne()
            .HasForeignKey<Contrato>(c => c.BudgetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Assinaturas)
            .WithOne()
            .HasForeignKey(s => s.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.TenantId, c.Numero })
            .IsUnique();

        builder.HasIndex(c => new { c.TenantId, c.BudgetId })
            .IsUnique()
            .HasFilter("[BudgetId] IS NOT NULL");
    }
}





