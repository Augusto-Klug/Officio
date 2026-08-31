using Officio.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal class ProspectoMap : IEntityTypeConfiguration<Prospecto>
{
    public void Configure(EntityTypeBuilder<Prospecto> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.RazaoSocial)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.Cnpj)
            .HasMaxLength(18);

        builder.Property(p => p.Cpf)
            .HasMaxLength(14);

        builder.HasMany(p => p.Socios)
            .WithOne()
            .HasForeignKey(s => s.ProspectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Budgets)
            .WithOne(b => b.Prospecto)
            .HasForeignKey(b => b.ProspectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.TenantId, p.Cnpj })
            .IsUnique()
            .HasFilter("[Cnpj] IS NOT NULL");
    }
}





