using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class OcrExecucao : EntidadeBase
{
    private readonly List<OcrCampoExtraido> _campos = [];

    public Guid DocumentoId { get; private set; }
    public string? Provedor { get; private set; }
    public string? IdExterno { get; private set; }
    public string StatusProcessamento { get; private set; } = "PENDENTE";
    public string? ModeloUtilizado { get; private set; }
    public string? PayloadRetorno { get; private set; }
    public string? CodigoErro { get; private set; }
    public string? MensagemErro { get; private set; }
    public DateTime IniciadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? ConcluidoEm { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public Documento Documento { get; private set; } = default!;
    public IReadOnlyCollection<OcrCampoExtraido> Campos => _campos.AsReadOnly();

    private OcrExecucao() { }

    public static OcrExecucao Criar(Guid documentoId, string? provedor = null)
        => new() { DocumentoId = documentoId, Provedor = provedor };
}
