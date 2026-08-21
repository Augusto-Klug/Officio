using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal class OrcamentoMap : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        builder.HasKey(orcamento => orcamento.Id);

        builder.Property(orcamento => orcamento.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(orcamento => orcamento.ValorMensalidade)
            .HasPrecision(18, 2);

        builder.Property(orcamento => orcamento.ValorPorFuncionario)
            .HasPrecision(18, 2);

        builder.Property(orcamento => orcamento.ValorTaxasExtras)
            .HasPrecision(18, 2);

        builder.Property(orcamento => orcamento.Desconto)
            .HasPrecision(18, 2);

        builder.Property(orcamento => orcamento.ValorTotal)
            .HasPrecision(18, 2);

        builder.Property<Guid?>("ContractId");

        builder.Property<DateTime?>("DataExpiracaoOrcamento");

        builder.Property<DateTime?>("DataInicio");

        builder.Property<int>("Numero");

        builder.Property<int?>("VigenciaMeses");

        builder.HasMany(orcamento => orcamento.Itens)
            .WithOne()
            .HasForeignKey(item => item.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(orcamento => orcamento.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(orcamento => new { orcamento.TenantId, orcamento.ProspectId });
    }
}
