using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.EnviarLinkFichaAdmissaoEmail;

public sealed class EnviarLinkFichaAdmissaoEmailHandler(IFichaAdmissaoServico servico)
    : IRequestHandler<EnviarLinkFichaAdmissaoEmailComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(EnviarLinkFichaAdmissaoEmailComando request, CancellationToken ct)
        => servico.EnviarLinkPorEmailAsync(request.Requisicao, ct);
}
