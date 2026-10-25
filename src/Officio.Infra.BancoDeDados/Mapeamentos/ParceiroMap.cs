using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal sealed class ParceiroMap : IEntityTypeConfiguration<Parceiro>
{
    public void Configure(EntityTypeBuilder<Parceiro> builder)
    {
        builder.ToTable("Parceiros");
        builder.HasKey(parceiro => parceiro.Id);
        builder.Property(parceiro => parceiro.Nome).HasMaxLength(300).IsRequired();
        builder.Property(parceiro => parceiro.RazaoSocial).HasMaxLength(300).IsRequired();
        builder.Property(parceiro => parceiro.NomeFantasia).HasMaxLength(300);
        builder.Property(parceiro => parceiro.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(parceiro => parceiro.Email).HasMaxLength(250);
        builder.Property(parceiro => parceiro.Telefone).HasMaxLength(30);
        builder.Property(parceiro => parceiro.Crc).HasMaxLength(50);
        builder.HasOne(parceiro => parceiro.Endereco).WithMany()
            .HasForeignKey(parceiro => parceiro.EnderecoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(parceiro => parceiro.Cnpj).IsUnique();
    }
}
