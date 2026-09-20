using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal class ItemCatalogoOrcamentoMap : IEntityTypeConfiguration<ItemCatalogoOrcamento>
{
    public void Configure(EntityTypeBuilder<ItemCatalogoOrcamento> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Descricao)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.Secao)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(item => item.Tipo)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(item => item.NaturezaCobranca)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(item => item.ValorReferencia)
            .HasPrecision(18, 2);

        builder.HasIndex(item => new { item.TenantId, item.Ativo, item.Secao });
    }
}
