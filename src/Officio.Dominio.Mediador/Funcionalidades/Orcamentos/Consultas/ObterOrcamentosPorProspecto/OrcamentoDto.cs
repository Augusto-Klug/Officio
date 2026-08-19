namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;

public record OrcamentoDto(
    Guid Id,
    Guid ProspectoId,
    string? Titulo,
    string Status,
    decimal ValorTotal,
    int DiaVencimento,
    string? Observacoes,
    string? MotivoRecusa,
    List<ItemOrcamentoDto> Itens);

public record ItemOrcamentoDto(
    Guid Id,
    Guid? ItemCatalogoOrcamentoId,
    string Descricao,
    string Secao,
    string Tipo,
    int Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    int Ordem);
