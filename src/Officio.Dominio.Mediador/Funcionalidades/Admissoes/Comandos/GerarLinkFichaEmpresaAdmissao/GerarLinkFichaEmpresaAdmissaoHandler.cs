using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.GerarLinkFichaEmpresaAdmissao;

public sealed class GerarLinkFichaEmpresaAdmissaoHandler(IFichaAdmissaoServico servico)
    : IRequestHandler<GerarLinkFichaEmpresaAdmissaoComando, Resultado<GerarLinkFichaAdmissaoDto>>
{
    public Task<Resultado<GerarLinkFichaAdmissaoDto>> Handle(
        GerarLinkFichaEmpresaAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.GerarLinkEmpresaAsync(request.AdmissaoId, cancellationToken);
}
