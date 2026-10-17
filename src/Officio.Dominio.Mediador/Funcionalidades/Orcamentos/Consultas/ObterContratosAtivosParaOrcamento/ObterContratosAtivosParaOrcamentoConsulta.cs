using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterContratosAtivosParaOrcamento;

public sealed record ObterContratosAtivosParaOrcamentoConsulta(Guid OrcamentoId)
    : IRequest<Resultado<List<ContratoAtivoResumoDto>>>;
