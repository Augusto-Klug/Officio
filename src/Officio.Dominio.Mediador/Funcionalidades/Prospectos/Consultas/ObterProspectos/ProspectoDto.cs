namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;

public record ProspectoDto(
    Guid Id,
    string RazaoSocial,
    string? Cnpj,
    string? Status,
    string? Responsavel,
    DateTime CriadoEm);





