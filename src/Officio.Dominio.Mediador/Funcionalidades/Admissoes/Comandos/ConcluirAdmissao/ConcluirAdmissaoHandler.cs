using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirAdmissao;

public class ConcluirAdmissaoHandler(IAdmissaoServico servico)
    : IRequestHandler<ConcluirAdmissaoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(ConcluirAdmissaoComando request, CancellationToken ct)
        => servico.ConcluirAsync(request.AdmissaoId, ct);
}
