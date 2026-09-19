using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.ReenviarConviteAdministrador;

public class ReenviarConviteAdministradorHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ReenviarConviteAdministradorComando, Resultado<ConviteCriadoDto>>
{
    public Task<Resultado<ConviteCriadoDto>> Handle(
        ReenviarConviteAdministradorComando request,
        CancellationToken ct) => servico.ReenviarConviteAdministradorAsync(request.EmpresaId, ct);
}
