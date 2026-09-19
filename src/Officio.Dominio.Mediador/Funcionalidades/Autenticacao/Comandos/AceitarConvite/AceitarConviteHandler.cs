using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.AceitarConvite;

public class AceitarConviteHandler(IGestaoAcessoServico servico)
    : IRequestHandler<AceitarConviteComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AceitarConviteComando request, CancellationToken ct)
        => servico.AceitarConviteAsync(new(request.Token, request.Senha), ct);
}
