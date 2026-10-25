using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas;
public record ObterRepresentantesContratoConsulta(Guid OrcamentoId) : IRequest<Resultado<RepresentantesContratoDto>>;
public class ObterRepresentantesContratoHandler(IContratoServico servico)
    : IRequestHandler<ObterRepresentantesContratoConsulta, Resultado<RepresentantesContratoDto>>
{
    public Task<Resultado<RepresentantesContratoDto>> Handle(ObterRepresentantesContratoConsulta request, CancellationToken ct)
        => servico.ObterRepresentantesAsync(request.OrcamentoId, ct);
}
