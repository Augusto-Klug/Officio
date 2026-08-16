using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Officio.Dominio.Entidades;

namespace Officio.Infra.BancoDeDados.Mapeamentos;

public class ValidacaoDocumentoMap : IEntityTypeConfiguration<ValidacaoDocumento>
{
    public void Configure(EntityTypeBuilder<ValidacaoDocumento> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.CamposExtraidos)
            .HasConversion(
                campos => JsonSerializer.Serialize(campos, (JsonSerializerOptions?)null),
                json => string.IsNullOrWhiteSpace(json)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("nvarchar(max)");

        builder.HasOne(v => v.Documento)
            .WithMany(d => d.Validacoes)
            .HasForeignKey(v => v.DocumentId);
    }
}
