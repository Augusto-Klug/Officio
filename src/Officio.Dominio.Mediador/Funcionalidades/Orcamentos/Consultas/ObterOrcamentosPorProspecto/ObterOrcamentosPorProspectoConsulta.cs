using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;

public record ObterOrcamentosPorProspectoConsulta(Guid ProspectoId)
    : IRequest<Resultado<List<OrcamentoDto>>>;
