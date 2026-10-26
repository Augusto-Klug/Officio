using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarAtivoItemCatalogoOrcamento;

public sealed record AlterarAtivoItemCatalogoOrcamentoComando(Guid ItemId, bool Ativo) : IRequest<Resultado<Guid>>;
