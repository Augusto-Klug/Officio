using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;

public record RecusarOrcamentoComando(Guid OrcamentoId, string Motivo) : IRequest<Resultado<bool>>;
