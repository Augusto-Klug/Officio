using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.AlterarPerfil;

public class AlterarPerfilUsuarioHandler(IGestaoAcessoServico servico)
    : IRequestHandler<AlterarPerfilUsuarioComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AlterarPerfilUsuarioComando request, CancellationToken ct)
        => servico.AlterarPerfilAsync(request.UsuarioId, request.Perfil, ct);
}
