using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class ValidacaoDocumento : EntidadeAuditavel
{
    public Guid DocumentId { get; private set; }
    public Guid? AdmissaoDocumentoId { get; private set; }
    public Guid? OcrExecucaoId { get; private set; }
    public Guid? StatusValidacaoDocumentoId { get; private set; }
    public Guid? RevisadoPorId { get; private set; }
    public DateTime? RevisadoEm { get; private set; }
    public string? ObservacaoRevisao { get; private set; }
    public StatusValidacao Status { get; private set; }
    public Dictionary<string, string> CamposExtraidos { get; private set; } = [];
    public decimal ConfiancaOcr { get; private set; }
    public bool FallbackManual { get; private set; }
    public string? Observacao { get; private set; }

    // Navigation
    public Documento Documento { get; private set; } = default!;
    public AdmissaoDocumento? AdmissaoDocumento { get; private set; }
    public OcrExecucao? OcrExecucao { get; private set; }
    public StatusValidacaoDocumento? StatusValidacaoDocumento { get; private set; }
    public Usuario? RevisadoPor { get; private set; }
    public ICollection<ValidacaoDivergencia> Divergencias { get; private set; } = [];

    private ValidacaoDocumento() { }

    public static ValidacaoDocumento Create(
        Guid tenantId,
        Guid documentId,
        Dictionary<string, string> camposExtraidos,
        List<object> divergencias,
        bool valido,
        decimal confiancaOcr,
        Guid? admissaoDocumentoId = null)
    {
        var validation = new ValidacaoDocumento
        {
            TenantId = tenantId,
            DocumentId = documentId,
            AdmissaoDocumentoId = admissaoDocumentoId,
            OcrExecucaoId = null,
            CamposExtraidos = camposExtraidos,
            ConfiancaOcr = confiancaOcr,
            Status = valido ? StatusValidacao.Aprovado : StatusValidacao.Reprovado,
            FallbackManual = false
        };

        validation.AddDomainEvent(new EventoDocumentoValidado(documentId, valido, tenantId));
        return validation;
    }

    public static ValidacaoDocumento RequireManualReview(
        Guid tenantId,
        Guid documentId,
        Dictionary<string, string>? camposExtraidos = null,
        decimal confiancaOcr = 0m,
        string? observacao = null,
        Guid? admissaoDocumentoId = null)
    {
        var validation = new ValidacaoDocumento
        {
            TenantId = tenantId,
            DocumentId = documentId,
            AdmissaoDocumentoId = admissaoDocumentoId,
            Status = StatusValidacao.RevisaoManual,
            CamposExtraidos = camposExtraidos ?? [],
            ConfiancaOcr = confiancaOcr,
            FallbackManual = true,
            Observacao = observacao
        };

        validation.AddDomainEvent(new EventoDocumentoRevisaoManual(documentId, tenantId));
        return validation;
    }

    public void ApproveManually(string? observacao)
    {
        Status = StatusValidacao.Aprovado;
        Observacao = observacao;
        RevisadoEm = DateTime.UtcNow;
        ObservacaoRevisao = observacao;
    }
}





