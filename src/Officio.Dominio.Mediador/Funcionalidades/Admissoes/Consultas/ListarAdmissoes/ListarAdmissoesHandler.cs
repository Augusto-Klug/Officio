using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarAdmissoes;

public class ListarAdmissoesHandler(IAdmissaoServico servico)
    : IRequestHandler<ListarAdmissoesConsulta, Resultado<List<AdmissaoResumoDto>>>
{
    public Task<Resultado<List<AdmissaoResumoDto>>> Handle(ListarAdmissoesConsulta request, CancellationToken ct)
        => servico.ListarAsync(request.ClienteId, request.Status, request.Busca, request.EmpresaId, ct);
}
