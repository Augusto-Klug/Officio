using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Infra.BancoDeDados.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

internal sealed class WebhookAutentiqueEventoMap : IEntityTypeConfiguration<WebhookAutentiqueEvento>
{
    public void Configure(EntityTypeBuilder<WebhookAutentiqueEvento> builder)
    {
        builder.ToTable("AutentiqueWebhookInbox");
        builder.HasKey(evento => evento.Id);
        builder.Property(evento => evento.HashPayload).HasMaxLength(64).IsRequired();
        builder.Property(evento => evento.Payload).HasColumnType("nvarchar(max)");
        builder.Property(evento => evento.TraceParent).HasMaxLength(100);
        builder.Property(evento => evento.TraceState).HasMaxLength(512);
        builder.Property(evento => evento.Status).HasConversion<int>();
        builder.Property(evento => evento.UltimoErro).HasMaxLength(2000);
        builder.HasIndex(evento => evento.HashPayload).IsUnique();
        builder.HasIndex(evento => new
        {
            evento.Status,
            evento.ProximaTentativaEm,
            evento.BloqueadoAte,
            evento.RecebidoEm
        });
    }
}
