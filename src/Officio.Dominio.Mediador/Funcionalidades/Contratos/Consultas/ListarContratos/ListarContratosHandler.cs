using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;

public class ListarContratosHandler(IContratoServico contratoServico)
    : IRequestHandler<ListarContratosConsulta, Resultado<List<ContratoDto>>>
{
    public async Task<Resultado<List<ContratoDto>>> Handle(
        ListarContratosConsulta request,
        CancellationToken ct)
        => await contratoServico.ListarAsync(request.EmpresaId, ct);
}
