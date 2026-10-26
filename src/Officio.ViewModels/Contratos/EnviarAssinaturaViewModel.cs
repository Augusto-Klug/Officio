using System.ComponentModel.DataAnnotations;

namespace Officio.ViewModels.Contratos;

public class EnviarAssinaturaViewModel
{
    [Required, EmailAddress]
    public string EmailContratante { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string EmailContratada { get; set; } = string.Empty;

    public string? MensagemPersonalizada { get; set; }
}
