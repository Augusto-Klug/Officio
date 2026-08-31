using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public record AlterarAtivoItemCatalogoOrcamentoComando(Guid ItemId, bool Ativo) : IRequest<Resultado<Guid>>;
