using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.GerarLinkFichaAdmissao;

public sealed class GerarLinkFichaAdmissaoHandler(IFichaAdmissaoServico servico)
    : IRequestHandler<GerarLinkFichaAdmissaoComando, Resultado<GerarLinkFichaAdmissaoDto>>
{
    public Task<Resultado<GerarLinkFichaAdmissaoDto>> Handle(
        GerarLinkFichaAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.GerarLinkAsync(request.AdmissaoId, cancellationToken);
}
