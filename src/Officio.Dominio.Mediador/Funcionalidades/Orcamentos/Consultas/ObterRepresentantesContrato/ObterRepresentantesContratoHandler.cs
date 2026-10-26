using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterRepresentantesContrato;

public sealed class ObterRepresentantesContratoHandler(IContratoServico servico)
    : IRequestHandler<ObterRepresentantesContratoConsulta, Resultado<RepresentantesContratoDto>>
{
    public Task<Resultado<RepresentantesContratoDto>> Handle(ObterRepresentantesContratoConsulta request, CancellationToken ct)
        => servico.ObterRepresentantesAsync(request.OrcamentoId, ct);
}
