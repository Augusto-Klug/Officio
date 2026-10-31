namespace Officio.Dominio.Contratos;

public record ResultadoPaginado<T>(
    List<T> Dados,
    int TotalRegistros,
    int Pagina,
    int TamanhoPagina,
    int TotalPaginas);
