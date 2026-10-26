using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterRepresentantesContrato;

public sealed record ObterRepresentantesContratoConsulta(Guid OrcamentoId)
    : IRequest<Resultado<RepresentantesContratoDto>>;
