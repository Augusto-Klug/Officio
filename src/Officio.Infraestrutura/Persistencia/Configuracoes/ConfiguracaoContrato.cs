using Officio.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Officio.Infraestrutura.Persistencia.Configuracoes;

public class ConfiguracaoContrato : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Numero)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.ValorTotal)
            .HasPrecision(18, 2);

        builder.HasMany(c => c.Assinaturas)
            .WithOne()
            .HasForeignKey(s => s.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.TenantId, c.Numero })
            .IsUnique();
    }
}





