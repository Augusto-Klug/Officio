using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public record CriarItemCatalogoOrcamentoComando(
    string Descricao,
    SecaoItemOrcamento Secao,
    TipoItemOrcamento Tipo,
    decimal? ValorReferencia) : IRequest<Resultado<Guid>>;
