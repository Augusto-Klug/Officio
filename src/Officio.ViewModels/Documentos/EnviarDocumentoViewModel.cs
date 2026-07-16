using Officio.ViewModels.Enumeradores;

namespace Officio.ViewModels.Documentos;

public class EnviarDocumentoViewModel
{
    public TipoDocumentoViewModel Tipo { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ProspectoId { get; set; }
    public Guid? SocioId { get; set; }
    public string? Observacoes { get; set; }
}
