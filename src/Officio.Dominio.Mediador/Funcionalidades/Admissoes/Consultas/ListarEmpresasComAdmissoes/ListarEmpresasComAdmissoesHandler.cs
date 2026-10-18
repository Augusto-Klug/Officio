using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarEmpresasComAdmissoes;

public class ListarEmpresasComAdmissoesHandler(IAdmissaoServico servico)
    : IRequestHandler<ListarEmpresasComAdmissoesConsulta, Resultado<List<EmpresaAdmissoesResumoDto>>>
{
    public Task<Resultado<List<EmpresaAdmissoesResumoDto>>> Handle(
        ListarEmpresasComAdmissoesConsulta request,
        CancellationToken ct)
        => servico.ListarEmpresasComAdmissoesAsync(request.Busca, ct);
}
