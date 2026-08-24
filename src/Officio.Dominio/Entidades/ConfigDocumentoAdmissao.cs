using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class ConfigDocumentoAdmissao : EntidadeBase
{
    public Guid TipoDocumentoId { get; private set; }
    public string? RotuloExibicao { get; private set; }
    public bool Obrigatorio { get; private set; }
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; } = true;
    public TipoDocumentoDominio TipoDocumento { get; private set; } = default!;

    private ConfigDocumentoAdmissao() { }

    public static ConfigDocumentoAdmissao Criar(Guid tipoDocumentoId, bool obrigatorio, int ordem, string? rotuloExibicao = null)
        => new() { TipoDocumentoId = tipoDocumentoId, Obrigatorio = obrigatorio, Ordem = ordem, RotuloExibicao = rotuloExibicao };
}
