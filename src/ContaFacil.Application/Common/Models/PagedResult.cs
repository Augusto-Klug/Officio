namespace ContaFacil.Application.Common.Models;

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
