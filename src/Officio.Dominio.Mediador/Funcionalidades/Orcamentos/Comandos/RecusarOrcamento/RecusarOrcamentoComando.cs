using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;

public record RecusarOrcamentoComando(Guid OrcamentoId, string Motivo) : IRequest<Resultado<Unit>>;
