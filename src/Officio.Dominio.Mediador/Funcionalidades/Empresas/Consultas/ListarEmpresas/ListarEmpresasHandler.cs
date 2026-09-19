using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Consultas.ListarEmpresas;

public class ListarEmpresasHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ListarEmpresasConsulta, Resultado<List<EmpresaPlataformaDto>>>
{
    public Task<Resultado<List<EmpresaPlataformaDto>>> Handle(
        ListarEmpresasConsulta request,
        CancellationToken ct) => servico.ListarEmpresasAsync(ct);
}
