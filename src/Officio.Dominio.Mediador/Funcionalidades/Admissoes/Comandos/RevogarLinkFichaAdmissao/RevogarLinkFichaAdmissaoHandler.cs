using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RevogarLinkFichaAdmissao;

public sealed class RevogarLinkFichaAdmissaoHandler(IFichaAdmissaoServico servico)
    : IRequestHandler<RevogarLinkFichaAdmissaoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(
        RevogarLinkFichaAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.RevogarLinkAsync(request.AdmissaoId, cancellationToken);
}
