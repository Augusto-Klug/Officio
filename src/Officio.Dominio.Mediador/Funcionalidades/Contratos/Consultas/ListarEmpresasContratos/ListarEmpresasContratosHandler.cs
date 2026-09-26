using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarEmpresasContratos;

public sealed class ListarEmpresasContratosHandler(IContratoServico contratoServico)
    : IRequestHandler<ListarEmpresasContratosConsulta, Resultado<List<EmpresaContratoFiltroDto>>>
{
    public Task<Resultado<List<EmpresaContratoFiltroDto>>> Handle(
        ListarEmpresasContratosConsulta request,
        CancellationToken cancellationToken)
        => contratoServico.ListarEmpresasAsync(cancellationToken);
}
