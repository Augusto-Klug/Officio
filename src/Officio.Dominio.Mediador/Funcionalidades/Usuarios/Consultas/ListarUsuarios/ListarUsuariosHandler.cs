using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Consultas.ListarUsuarios;

public class ListarUsuariosHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ListarUsuariosConsulta, Resultado<List<UsuarioGestaoDto>>>
{
    public Task<Resultado<List<UsuarioGestaoDto>>> Handle(ListarUsuariosConsulta request, CancellationToken ct)
        => servico.ListarUsuariosAsync(ct);
}
