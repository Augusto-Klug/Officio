using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;
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
    public string? TipoDocumentoDetectado => CamposExtraidos.GetValueOrDefault("__tipo_documento_detectado");
    public bool TipoDocumentoIncompativel =>
        CamposExtraidos.GetValueOrDefault("__tipo_documento_incompativel") == "true";

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
        IReadOnlyCollection<DivergenciaDocumento> divergencias,
        bool valido,
        decimal confiancaOcr,
        Guid? admissaoDocumentoId = null,
        Guid? ocrExecucaoId = null,
        string? observacao = null)
    {
        var validation = new ValidacaoDocumento
        {
            TenantId = tenantId,
            DocumentId = documentId,
            AdmissaoDocumentoId = admissaoDocumentoId,
            OcrExecucaoId = ocrExecucaoId,
            CamposExtraidos = camposExtraidos,
            ConfiancaOcr = confiancaOcr,
            Status = valido ? StatusValidacao.Aprovado : StatusValidacao.Reprovado,
            FallbackManual = false,
            Observacao = observacao
        };

        validation.AdicionarDivergencias(divergencias);

        validation.AddDomainEvent(new EventoDocumentoValidado(documentId, valido, tenantId));
        return validation;
    }

    public static ValidacaoDocumento RequireManualReview(
        Guid tenantId,
        Guid documentId,
        Dictionary<string, string>? camposExtraidos = null,
        decimal confiancaOcr = 0m,
        string? observacao = null,
        Guid? admissaoDocumentoId = null,
        IReadOnlyCollection<DivergenciaDocumento>? divergencias = null,
        Guid? ocrExecucaoId = null)
    {
        var validation = new ValidacaoDocumento
        {
            TenantId = tenantId,
            DocumentId = documentId,
            AdmissaoDocumentoId = admissaoDocumentoId,
            OcrExecucaoId = ocrExecucaoId,
            Status = StatusValidacao.RevisaoManual,
            CamposExtraidos = camposExtraidos ?? [],
            ConfiancaOcr = confiancaOcr,
            FallbackManual = true,
            Observacao = observacao
        };

        validation.AdicionarDivergencias(divergencias ?? []);

        validation.AddDomainEvent(new EventoDocumentoRevisaoManual(documentId, tenantId));
        return validation;
    }

    public void ApproveManually(Guid revisorId, string observacao)
    {
        if (revisorId == Guid.Empty)
            throw new ArgumentException("O usuario revisor deve ser informado.", nameof(revisorId));
        if (string.IsNullOrWhiteSpace(observacao))
            throw new ArgumentException("A justificativa da revisao manual deve ser informada.", nameof(observacao));

        Status = StatusValidacao.Aprovado;
        RevisadoPorId = revisorId;
        RevisadoEm = DateTime.UtcNow;
        ObservacaoRevisao = observacao.Trim();
    }

    private void AdicionarDivergencias(IEnumerable<DivergenciaDocumento> divergencias)
    {
        foreach (var divergencia in divergencias)
        {
            Divergencias.Add(ValidacaoDivergencia.Criar(
                Id,
                divergencia.Campo,
                string.IsNullOrWhiteSpace(divergencia.ValorDocumento) ? "CAMPO_NAO_EXTRAIDO" : "VALOR_DIVERGENTE",
                divergencia.ValorCadastro,
                divergencia.ValorDocumento,
                divergencia.Similaridade));
        }
    }
}





