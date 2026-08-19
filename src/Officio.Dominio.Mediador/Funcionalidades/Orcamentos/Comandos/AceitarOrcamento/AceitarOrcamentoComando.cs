using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;

public record AceitarOrcamentoComando(Guid OrcamentoId) : IRequest<Resultado<Unit>>;
