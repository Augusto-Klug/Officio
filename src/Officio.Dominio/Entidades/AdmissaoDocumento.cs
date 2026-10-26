using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class AdmissaoDocumento : EntidadeBase
{
    public Guid AdmissaoId { get; private set; }
    public Guid TipoDocumentoId { get; private set; }
    public Guid? DocumentoId { get; private set; }
    public string? RotuloExibicao { get; private set; }
    public bool Obrigatorio { get; private set; }
    public bool RevisaoPendente { get; private set; }
    public int Ordem { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Admissao Admissao { get; private set; } = default!;
    public TipoDocumentoDominio TipoDocumento { get; private set; } = default!;
    public Documento? Documento { get; private set; }

    private AdmissaoDocumento() { }

    public static AdmissaoDocumento Criar(Guid admissaoId, Guid tipoDocumentoId, bool obrigatorio, int ordem, string? rotuloExibicao = null)
        => new()
        {
            AdmissaoId = admissaoId,
            TipoDocumentoId = tipoDocumentoId,
            Obrigatorio = obrigatorio,
            Ordem = ordem,
            RotuloExibicao = string.IsNullOrWhiteSpace(rotuloExibicao) ? null : rotuloExibicao.Trim()
        };

    public void VincularDocumento(Documento documento)
    {
        Documento = documento;
        DocumentoId = documento.Id;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void RemoverDocumento()
    {
        Documento = null;
        DocumentoId = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void MarcarRevisaoPendente()
    {
        RevisaoPendente = DocumentoId.HasValue;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void ConcluirRevisao()
    {
        RevisaoPendente = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}
