using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarOrcamento;

public record AtualizarOrcamentoComando(
    Guid OrcamentoId,
    string? Titulo,
    int DiaVencimento,
    string? Observacoes,
    List<CriarItemOrcamentoDto> Itens) : IRequest<Resultado<Guid>>;
