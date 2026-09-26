using Officio.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal class ClienteMap : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.RazaoSocial)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(c => c.Cnpj)
            .HasMaxLength(18);

        builder.Property(c => c.SciCodigo)
            .HasMaxLength(50);

        builder.Property(c => c.EmpresaId)
            .IsRequired();

        builder.HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Endereco)
            .WithMany()
            .HasForeignKey(c => c.EnderecoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Socios)
            .WithOne()
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Contratos)
            .WithOne(ct => ct.Cliente)
            .HasForeignKey(ct => ct.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documentos)
            .WithOne()
            .HasForeignKey(d => d.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => new { c.TenantId, c.Cnpj })
            .IsUnique()
            .HasFilter("[Cnpj] IS NOT NULL");

        builder.HasIndex(c => new { c.TenantId, c.SciCodigo })
            .IsUnique()
            .HasFilter("[SciCodigo] IS NOT NULL");

        builder.HasIndex(c => new { c.TenantId, c.ProspectOrigemId })
            .IsUnique()
            .HasFilter("[ProspectOrigemId] IS NOT NULL");
    }
}





