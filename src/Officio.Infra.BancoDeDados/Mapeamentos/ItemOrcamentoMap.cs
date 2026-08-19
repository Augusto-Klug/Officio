using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal class ItemOrcamentoMap : IEntityTypeConfiguration<ItemOrcamento>
{
    public void Configure(EntityTypeBuilder<ItemOrcamento> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Descricao)
            .IsRequired();

        builder.Property(item => item.Secao)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(item => item.Tipo)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(item => item.ValorUnitario)
            .HasPrecision(18, 2);

        builder.HasIndex(item => new { item.TenantId, item.OrcamentoId, item.Ordem });
    }
}
