using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Documento : EntidadeAuditavel
{
    public string NomeArquivo { get; private set; } = default!;
    public TipoDocumento Tipo { get; private set; }
    public string CaminhoStorage { get; private set; } = default!;
    public decimal QualidadeEstimada { get; private set; }
    public bool Legivel { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid? ProspectId { get; private set; }
    public Guid? SocioId { get; private set; }
    public string? Observacoes { get; private set; }

    // Navigation
    public ICollection<ValidacaoDocumento> Validacoes { get; private set; } = [];

    private Documento() { }

    public static Documento Create(
        Guid tenantId,
        string nomeArquivo,
        TipoDocumento tipo,
        string caminhoStorage,
        decimal qualidadeEstimada,
        Guid? clientId = null,
        Guid? prospectId = null,
        Guid? socioId = null,
        string? observacoes = null)
    {
        return new Documento
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





