using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentoPorId;

public record ObterOrcamentoPorIdConsulta(Guid OrcamentoId) : IRequest<Resultado<OrcamentoDto>>;
