namespace Officio.Dominio.Contratos;

public record ProspectoDetalheDto(
    Guid Id,
    string Status,
    CriarProspectoRequisicao Dados,
    IReadOnlyList<ContratoCancelavelDto>? ContratosCancelaveis = null);
