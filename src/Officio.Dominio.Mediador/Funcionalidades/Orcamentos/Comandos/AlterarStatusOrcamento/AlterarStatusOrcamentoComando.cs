using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;

public record AlterarStatusOrcamentoComando(Guid OrcamentoId, StatusOrcamento Status)
    : IRequest<Resultado<bool>>;
