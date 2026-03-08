using ContaFacil.Domain.Common;
using ContaFacil.Domain.Enums;
using ContaFacil.Domain.Events;

namespace ContaFacil.Domain.Entities;

public class DocumentValidation : AuditableEntity
{
    public Guid DocumentId { get; private set; }
    public ValidationStatus Status { get; private set; }
    public Dictionary<string, string> CamposExtraidos { get; private set; } = [];
    public decimal ConfiancaOcr { get; private set; }
    public bool FallbackManual { get; private set; }
    public string? Observacao { get; private set; }

    // Navigation
    public Document Document { get; private set; } = default!;

    private DocumentValidation() { }

    public static DocumentValidation Create(
        Guid documentId,
        Dictionary<string, string> camposExtraidos,
        List<object> divergencias,
        bool valido,
        decimal confiancaOcr)
    {
        var validation = new DocumentValidation
        {
            DocumentId = documentId,
            CamposExtraidos = camposExtraidos,
            ConfiancaOcr = confiancaOcr,
            Status = valido ? ValidationStatus.Aprovado : ValidationStatus.Reprovado,
            FallbackManual = false
        };

        validation.AddDomainEvent(new DocumentValidatedEvent(documentId, valido));
        return validation;
    }

    public static DocumentValidation RequireManualReview(Guid documentId)
    {
        return new DocumentValidation
        {
            DocumentId = documentId,
            Status = ValidationStatus.RevisaoManual,
            FallbackManual = true
        };
    }

    public void ApproveManually(string? observacao)
    {
        Status = ValidationStatus.Aprovado;
        Observacao = observacao;
    }
}
