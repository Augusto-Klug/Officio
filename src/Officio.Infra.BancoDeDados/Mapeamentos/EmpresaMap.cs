using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal sealed class EmpresaMap : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");
        builder.HasKey(empresa => empresa.Id);
        builder.Property(empresa => empresa.TenantId).HasColumnName("ParceiroId");
        builder.Property(empresa => empresa.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(empresa => empresa.RazaoSocial).HasMaxLength(300).IsRequired();
        builder.Property(empresa => empresa.Nome).HasMaxLength(300).IsRequired();
        builder.Property(empresa => empresa.NomeFantasia).HasMaxLength(300);
        builder.Property(empresa => empresa.Email).HasMaxLength(250);
        builder.Property(empresa => empresa.Whatsapp).HasMaxLength(30);
        builder.HasOne(empresa => empresa.Parceiro)
            .WithMany(parceiro => parceiro.Empresas)
            .HasForeignKey(empresa => empresa.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(empresa => empresa.Endereco)
            .WithMany()
            .HasForeignKey(empresa => empresa.EnderecoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(empresa => new { empresa.TenantId, empresa.Cnpj })
            .IsUnique()
            .HasFilter("[Cnpj] <> ''");
    }
}
