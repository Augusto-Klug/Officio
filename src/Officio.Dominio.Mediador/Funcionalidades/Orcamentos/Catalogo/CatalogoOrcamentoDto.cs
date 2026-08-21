namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public record CatalogoOrcamentoDto(
    Guid Id,
    string Descricao,
    string Secao,
    string Tipo,
    decimal? ValorReferencia,
    bool Ativo);
