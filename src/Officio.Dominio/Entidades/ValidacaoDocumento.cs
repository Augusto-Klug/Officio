using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class ValidacaoDocumento : EntidadeAuditavel
{
    public Guid DocumentId { get; private set; }
    public StatusValidacao Status { get; private set; }
    public Dictionary<string, string> CamposExtraidos { get; private set; } = [];
    public decimal ConfiancaOcr { get; private set; }
    public bool FallbackManual { get; private set; }
    public string? Observacao { get; private set; }

    // Navigation
    public Documento Documento { get; private set; } = default!;

    private ValidacaoDocumento() { }

    public static ValidacaoDocumento Create(
        Guid tenantId,
        Guid documentId,
        Dictionary<string, string> camposExtraidos,
        List<object> divergencias,
        bool valido,
        decimal confiancaOcr)
    {
        var validation = new ValidacaoDocumento
        {
            TenantId = tenantId,
            DocumentId = documentId,
            CamposExtraidos = camposExtraidos,
            ConfiancaOcr = confiancaOcr,
            Status = valido ? StatusValidacao.Aprovado : StatusValidacao.Reprovado,
            FallbackManual = false
        };

        validation.AddDomainEvent(new EventoDocumentoValidado(documentId, valido));
        return validation;
    }

    public static ValidacaoDocumento RequireManualReview(
        Guid tenantId,
        Guid documentId,
        Dictionary<string, string>? camposExtraidos = null,
        decimal confiancaOcr = 0m,
        string? observacao = null)
    {
        return new ValidacaoDocumento
        {
            TenantId = tenantId,
            DocumentId = documentId,
            Status = StatusValidacao.RevisaoManual,
            CamposExtraidos = camposExtraidos ?? [],
            ConfiancaOcr = confiancaOcr,
            FallbackManual = true,
            Observacao = observacao
        };
    }

    public void ApproveManually(string? observacao)
    {
        Status = StatusValidacao.Aprovado;
        Observacao = observacao;
    }
}





