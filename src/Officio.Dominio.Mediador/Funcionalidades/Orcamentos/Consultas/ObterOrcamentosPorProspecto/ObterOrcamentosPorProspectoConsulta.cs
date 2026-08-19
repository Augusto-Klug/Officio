using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;

public record ObterOrcamentosPorProspectoConsulta(Guid ProspectoId)
    : IRequest<Resultado<List<OrcamentoDto>>>;
