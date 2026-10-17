using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterContratosAtivosParaOrcamento;

public sealed class ObterContratosAtivosParaOrcamentoHandler(IContratoServico contratoServico)
    : IRequestHandler<ObterContratosAtivosParaOrcamentoConsulta, Resultado<List<ContratoAtivoResumoDto>>>
{
    public Task<Resultado<List<ContratoAtivoResumoDto>>> Handle(
        ObterContratosAtivosParaOrcamentoConsulta request,
        CancellationToken cancellationToken)
        => contratoServico.ListarAtivosParaOrcamentoAsync(request.OrcamentoId, cancellationToken);
}
