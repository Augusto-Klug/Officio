using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;

public class ObterContratoPorIdHandler(IContratoServico contratoServico)
    : IRequestHandler<ObterContratoPorIdConsulta, Resultado<ContratoDto>>
{
    public async Task<Resultado<ContratoDto>> Handle(
        ObterContratoPorIdConsulta request,
        CancellationToken ct)
        => await contratoServico.ObterPorIdAsync(request.ContratoId, ct);
}
