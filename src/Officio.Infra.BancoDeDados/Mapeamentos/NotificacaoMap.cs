using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal sealed class NotificacaoMap : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.Property(notificacao => notificacao.Tipo)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(notificacao => notificacao.Titulo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(notificacao => notificacao.Mensagem)
            .HasMaxLength(1000);

        builder.HasIndex(notificacao => new
            {
                notificacao.TenantId,
                notificacao.UserId,
                notificacao.Tipo,
                notificacao.ReferenciaId
            })
            .IsUnique()
            .HasFilter("[ReferenciaId] IS NOT NULL");
    }
}
