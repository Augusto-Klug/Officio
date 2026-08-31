namespace Officio.Dominio.Contratos.Dtos;

public record ProspectoDto(
    Guid Id,
    string RazaoSocial,
    string? Cnpj,
    string? Status,
    string? Responsavel,
    DateTime CriadoEm);

public record ProspectoCriadoDto(Guid Id, string RazaoSocial);

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
