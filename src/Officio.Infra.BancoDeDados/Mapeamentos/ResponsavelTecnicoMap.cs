using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal sealed class ResponsavelTecnicoMap : IEntityTypeConfiguration<ResponsavelTecnico>
{
    public void Configure(EntityTypeBuilder<ResponsavelTecnico> builder)
    {
        builder.ToTable("ResponsaveisTecnicos");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nome).HasMaxLength(250).IsRequired();
        builder.Property(r => r.Cpf).HasMaxLength(11).IsRequired();
        builder.Property(r => r.Profissao).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Crc).HasMaxLength(50).IsRequired();
        builder.HasOne(r => r.Parceiro).WithOne(p => p.ResponsavelTecnico)
            .HasForeignKey<ResponsavelTecnico>(r => r.ParceiroId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => r.ParceiroId).IsUnique();
        builder.HasOne(r => r.Endereco).WithMany().HasForeignKey(r => r.EnderecoId).OnDelete(DeleteBehavior.Restrict);
    }
}
