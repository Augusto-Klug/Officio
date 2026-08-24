using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class ContratoVersao : EntidadeAuditavel
{
    public Guid ContratoId { get; private set; }
    public int NumeroVersao { get; private set; }
    public string Conteudo { get; private set; } = default!;
    public Guid? DocumentoId { get; private set; }
    public DateTime GeradoEm { get; private set; } = DateTime.UtcNow;
    public Contrato Contrato { get; private set; } = default!;
    public Documento? Documento { get; private set; }

    private ContratoVersao() { }

    public static ContratoVersao Criar(Guid tenantId, Guid contratoId, int numeroVersao, string conteudo, Guid? documentoId = null)
        => new()
        {
            TenantId = tenantId,
            ContratoId = contratoId,
            NumeroVersao = numeroVersao,
            Conteudo = conteudo,
            DocumentoId = documentoId
        };
}
