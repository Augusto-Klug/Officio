namespace Officio.Dominio.Contratos;

public record ObterProspectosFiltro(
    int Pagina,
    int TamanhoPagina,
    string? Status,
    string? Busca,
    Guid? ResponsavelId);
