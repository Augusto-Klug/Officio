using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarItemCatalogoOrcamento;

public sealed record CriarItemCatalogoOrcamentoComando(
    string Descricao,
    SecaoItemOrcamento Secao,
    TipoItemOrcamento Tipo,
    decimal? ValorReferencia,
    NaturezaCobrancaItemOrcamento NaturezaCobranca = NaturezaCobrancaItemOrcamento.NaoRecorrente)
    : IRequest<Resultado<Guid>>;
