namespace Officio.ViewModels.Contratos;

public class EnviarAssinaturaViewModel
{
    public List<SignatarioViewModel> Signatarios { get; set; } = [];
    public string? MensagemPersonalizada { get; set; }
}

public class SignatarioViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
