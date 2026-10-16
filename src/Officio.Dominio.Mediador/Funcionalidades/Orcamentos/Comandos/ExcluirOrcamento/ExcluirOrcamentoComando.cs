using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ExcluirOrcamento;

public record ExcluirOrcamentoComando(Guid OrcamentoId) : IRequest<Resultado<bool>>;
