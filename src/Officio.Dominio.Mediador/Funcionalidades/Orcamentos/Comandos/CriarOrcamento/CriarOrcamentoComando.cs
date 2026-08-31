using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;

public record CriarOrcamentoComando(
    Guid ProspectoId,
    string? Titulo,
    int DiaVencimento,
    string? Observacoes,
    List<CriarItemOrcamentoDto> Itens) : IRequest<Resultado<Guid>>;

public record CriarItemOrcamentoDto(
    Guid? ItemCatalogoOrcamentoId,
    string? Descricao,
    SecaoItemOrcamento? Secao,
    TipoItemOrcamento? Tipo,
    int Quantidade,
    decimal ValorUnitario,
    int Ordem);
