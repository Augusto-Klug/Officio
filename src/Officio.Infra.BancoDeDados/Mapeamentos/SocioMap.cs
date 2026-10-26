using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;
namespace Officio.Infra.BancoDeDados.Mapeamentos;
internal class SocioMap : IEntityTypeConfiguration<Socio>
{
    public void Configure(EntityTypeBuilder<Socio> builder)
    {
        builder.Ignore(s => s.Nome);
        builder.Ignore(s => s.Cpf);
        builder.Property(s => s.Participacao).HasPrecision(5, 2);
        builder.HasOne(s => s.Pessoa).WithMany().HasForeignKey(s => s.PessoaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.TipoRelacao).WithMany().HasForeignKey(s => s.TipoRelacaoId).OnDelete(DeleteBehavior.Restrict);
    }
}
