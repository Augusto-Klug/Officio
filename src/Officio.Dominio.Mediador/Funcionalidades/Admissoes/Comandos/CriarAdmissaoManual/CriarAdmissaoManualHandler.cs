using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CriarAdmissaoManual;

public class CriarAdmissaoManualHandler(IAdmissaoServico servico)
    : IRequestHandler<CriarAdmissaoManualComando, Resultado<Guid>>
{
    public Task<Resultado<Guid>> Handle(CriarAdmissaoManualComando request, CancellationToken ct)
        => servico.CriarManualAsync(new CriarAdmissaoManualRequisicao(request.ClienteId, request.Dados), ct);
}
