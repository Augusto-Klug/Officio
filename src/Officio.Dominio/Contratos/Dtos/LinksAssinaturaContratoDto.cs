namespace Officio.Dominio.Contratos.Dtos;

public sealed record LinksAssinaturaContratoDto(
    IReadOnlyList<LinkAssinaturaSignatarioDto> Signatarios);
