namespace Officio.Dominio.Contratos.Dtos;

public sealed record LinkAssinaturaSignatarioDto(
    string Tipo,
    string Nome,
    string Email,
    bool Assinado,
    string? Link);
