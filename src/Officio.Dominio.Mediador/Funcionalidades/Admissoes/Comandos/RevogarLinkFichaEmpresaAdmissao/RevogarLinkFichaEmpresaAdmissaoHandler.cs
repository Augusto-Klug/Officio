using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RevogarLinkFichaEmpresaAdmissao;

public sealed class RevogarLinkFichaEmpresaAdmissaoHandler(IFichaAdmissaoServico servico)
    : IRequestHandler<RevogarLinkFichaEmpresaAdmissaoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(
        RevogarLinkFichaEmpresaAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.RevogarLinkEmpresaAsync(request.AdmissaoId, cancellationToken);
}
