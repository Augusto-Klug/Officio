using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;

public record AlterarStatusOrcamentoComando(Guid OrcamentoId, StatusOrcamento Status)
    : IRequest<Resultado<Unit>>;
