using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Consultas.ListarConvites;

public class ListarConvitesHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ListarConvitesConsulta, Resultado<List<ConviteUsuarioDto>>>
{
    public Task<Resultado<List<ConviteUsuarioDto>>> Handle(ListarConvitesConsulta request, CancellationToken ct)
        => servico.ListarConvitesAsync(ct);
}
