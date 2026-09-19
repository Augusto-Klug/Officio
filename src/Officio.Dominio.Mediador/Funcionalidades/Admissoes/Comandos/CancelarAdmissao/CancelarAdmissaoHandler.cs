using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CancelarAdmissao;

public class CancelarAdmissaoHandler(IAdmissaoServico servico)
    : IRequestHandler<CancelarAdmissaoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(CancelarAdmissaoComando request, CancellationToken ct)
        => servico.CancelarAsync(request.AdmissaoId, ct);
}
