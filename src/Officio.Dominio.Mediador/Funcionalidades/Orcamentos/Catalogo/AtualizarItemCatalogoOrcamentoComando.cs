using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public record AtualizarItemCatalogoOrcamentoComando(
    Guid ItemId,
    string Descricao,
    SecaoItemOrcamento Secao,
    TipoItemOrcamento Tipo,
    decimal? ValorReferencia) : IRequest<Resultado<Guid>>;
