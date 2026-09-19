using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.ReenviarConvite;

public class ReenviarConviteHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ReenviarConviteComando, Resultado<ConviteCriadoDto>>
{
    public Task<Resultado<ConviteCriadoDto>> Handle(ReenviarConviteComando request, CancellationToken ct)
        => servico.ReenviarConviteAsync(request.ConviteId, ct);
}
