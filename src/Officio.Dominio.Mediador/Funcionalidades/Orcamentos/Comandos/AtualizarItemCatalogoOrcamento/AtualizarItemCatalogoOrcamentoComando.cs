using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarItemCatalogoOrcamento;

public sealed record AtualizarItemCatalogoOrcamentoComando(
    Guid ItemId,
    string Descricao,
    SecaoItemOrcamento Secao,
    TipoItemOrcamento Tipo,
    decimal? ValorReferencia,
    NaturezaCobrancaItemOrcamento NaturezaCobranca = NaturezaCobrancaItemOrcamento.NaoRecorrente)
    : IRequest<Resultado<Guid>>;
