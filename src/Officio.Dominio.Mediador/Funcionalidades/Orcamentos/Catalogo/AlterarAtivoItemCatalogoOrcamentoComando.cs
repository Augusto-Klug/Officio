using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public record AlterarAtivoItemCatalogoOrcamentoComando(Guid ItemId, bool Ativo) : IRequest<Resultado<Guid>>;
