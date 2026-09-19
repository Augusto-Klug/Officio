using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.CancelarConvite;

public class CancelarConviteHandler(IGestaoAcessoServico servico)
    : IRequestHandler<CancelarConviteComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(CancelarConviteComando request, CancellationToken ct)
        => servico.CancelarConviteAsync(request.ConviteId, ct);
}
