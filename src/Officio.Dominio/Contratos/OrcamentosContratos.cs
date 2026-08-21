using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Contratos;

public record CriarOrcamentoRequisicao(
    Guid ProspectoId,
    string? Titulo,
    int DiaVencimento,
    string? Observacoes,
    List<ItemOrcamentoRequisicao> Itens);

public record AtualizarOrcamentoRequisicao(
    Guid OrcamentoId,
    string? Titulo,
    int DiaVencimento,
    string? Observacoes,
    List<ItemOrcamentoRequisicao> Itens);

public record ItemOrcamentoRequisicao(
    Guid? ItemCatalogoOrcamentoId,
    string? Descricao,
    SecaoItemOrcamento? Secao,
    TipoItemOrcamento? Tipo,
    int Quantidade,
    decimal ValorUnitario,
    int Ordem);

public record CriarItemCatalogoOrcamentoRequisicao(
    string Descricao,
    SecaoItemOrcamento Secao,
    TipoItemOrcamento Tipo,
    decimal? ValorReferencia);

public record AtualizarItemCatalogoOrcamentoRequisicao(
    Guid ItemId,
    string Descricao,
    SecaoItemOrcamento Secao,
    TipoItemOrcamento Tipo,
    decimal? ValorReferencia);

public record AlterarAtivoItemCatalogoOrcamentoRequisicao(Guid ItemId, bool Ativo);

public record AlterarStatusOrcamentoRequisicao(Guid OrcamentoId, StatusOrcamento Status);

public record RecusarOrcamentoRequisicao(Guid OrcamentoId, string Motivo);

public record OrcamentoResultado(
    Guid Id,
    Guid ProspectoId,
    string? Titulo,
    string Status,
    decimal ValorTotal,
    int DiaVencimento,
    string? Observacoes,
    string? MotivoRecusa,
    List<ItemOrcamentoResultado> Itens);

public record ItemOrcamentoResultado(
    Guid Id,
    Guid? ItemCatalogoOrcamentoId,
    string Descricao,
    string Secao,
    string Tipo,
    int Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    int Ordem);

public record CatalogoOrcamentoResultado(
    Guid Id,
    string Descricao,
    string Secao,
    string Tipo,
    decimal? ValorReferencia,
    bool Ativo);
