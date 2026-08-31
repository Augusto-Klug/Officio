using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;

public record AceitarOrcamentoComando(Guid OrcamentoId) : IRequest<Resultado<bool>>;
