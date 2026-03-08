namespace ContaFacil.Application.Features.Prospects.Queries.GetProspects;

public record ProspectDto(
    Guid Id,
    string RazaoSocial,
    string? Cnpj,
    string? Status,
    string? Responsavel,
    DateTime CriadoEm);
