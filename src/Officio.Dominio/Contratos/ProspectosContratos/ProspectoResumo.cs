namespace Officio.Dominio.Contratos;

public record ProspectoResumo(
    Guid Id,
    string RazaoSocial,
    string? Cnpj,
    string Status,
    string? ResponsavelNome,
    DateTime CriadoEm,
    bool PodeEditar,
    bool PodeExcluir,
    bool AguardandoAssinatura);
