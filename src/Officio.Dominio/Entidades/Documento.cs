using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Documento : EntidadeAuditavel
{
    public Guid? TipoDocumentoId { get; private set; }
    public string NomeArquivo { get; private set; } = default!;
    public TipoDocumento Tipo { get; private set; }
    public string CaminhoStorage { get; private set; } = default!;
    public string? MimeType { get; private set; }
    public long? TamanhoBytes { get; private set; }
    public string Origem { get; private set; } = "GERAL";
    public decimal QualidadeEstimada { get; private set; }
    public bool Legivel { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid? ProspectId { get; private set; }
    public Guid? SocioId { get; private set; }
    public Guid? EmpresaId { get; private set; }
    public Guid? ContratoId { get; private set; }
    public Guid? AdmissaoId { get; private set; }
    public Guid? EnviadoPorId { get; private set; }
    public string? Observacoes { get; private set; }

    // Navigation
    public TipoDocumentoDominio? TipoDocumento { get; private set; }
    public Empresa? Empresa { get; private set; }
    public Contrato? Contrato { get; private set; }
    public Admissao? Admissao { get; private set; }
    public Usuario? EnviadoPor { get; private set; }
    public ICollection<ValidacaoDocumento> Validacoes { get; private set; } = [];
    public ICollection<OcrExecucao> ExecucoesOcr { get; private set; } = [];

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

    public static Documento CriarContratoAssinado(
        Guid tenantId,
        Guid contratoId,
        Guid? empresaId,
        Guid? clientId,
        Guid prospectId,
        string nomeArquivo,
        string caminhoStorage,
        long? tamanhoBytes = null)
    {
        return new Documento
        {
            TenantId = tenantId,
            NomeArquivo = nomeArquivo,
            Tipo = Enumeradores.TipoDocumento.ContratoSocial,
            CaminhoStorage = caminhoStorage,
            MimeType = "application/pdf",
            TamanhoBytes = tamanhoBytes,
            Origem = "ASSINATURA_AUTENTIQUE",
            QualidadeEstimada = 1m,
            Legivel = true,
            EmpresaId = empresaId,
            ClientId = clientId,
            ProspectId = prospectId,
            ContratoId = contratoId,
            Observacoes = "PDF final assinado retornado pela Autentique."
        };
    }

    public static Documento CriarAdmissao(
        Guid tenantId,
        Guid admissaoId,
        Guid empresaId,
        Guid tipoDocumentoId,
        string nomeArquivo,
        TipoDocumento tipo,
        string caminhoStorage,
        string mimeType,
        long tamanhoBytes,
        decimal qualidadeEstimada)
        => new()
        {
            TenantId = tenantId,
            AdmissaoId = admissaoId,
            EmpresaId = empresaId,
            TipoDocumentoId = tipoDocumentoId,
            NomeArquivo = nomeArquivo,
            Tipo = tipo,
            CaminhoStorage = caminhoStorage,
            MimeType = mimeType,
            TamanhoBytes = tamanhoBytes,
            Origem = "ADMISSAO",
            QualidadeEstimada = qualidadeEstimada,
            Legivel = qualidadeEstimada >= 0.4m
        };
}





