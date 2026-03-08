using ContaFacil.Domain.Common;
using ContaFacil.Domain.Enums;

namespace ContaFacil.Domain.Entities;

public class Document : AuditableEntity
{
    public string NomeArquivo { get; private set; } = default!;
    public DocumentType Tipo { get; private set; }
    public string CaminhoStorage { get; private set; } = default!;
    public decimal QualidadeEstimada { get; private set; }
    public bool Legivel { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid? ProspectId { get; private set; }
    public Guid? SocioId { get; private set; }
    public string? Observacoes { get; private set; }

    // Navigation
    public ICollection<DocumentValidation> Validacoes { get; private set; } = [];

    private Document() { }

    public static Document Create(
        Guid tenantId,
        string nomeArquivo,
        DocumentType tipo,
        string caminhoStorage,
        decimal qualidadeEstimada,
        Guid? clientId = null,
        Guid? prospectId = null,
        Guid? socioId = null,
        string? observacoes = null)
    {
        return new Document
        {
            TenantId = tenantId,
            NomeArquivo = nomeArquivo,
            Tipo = tipo,
            CaminhoStorage = caminhoStorage,
            QualidadeEstimada = qualidadeEstimada,
            Legivel = qualidadeEstimada >= 0.4m,
            ClientId = clientId,
            ProspectId = prospectId,
            SocioId = socioId,
            Observacoes = observacoes
        };
    }
}
