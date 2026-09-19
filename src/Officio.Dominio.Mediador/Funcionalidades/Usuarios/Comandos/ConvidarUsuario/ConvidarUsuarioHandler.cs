using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.ConvidarUsuario;

public class ConvidarUsuarioHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ConvidarUsuarioComando, Resultado<ConviteCriadoDto>>
{
    public Task<Resultado<ConviteCriadoDto>> Handle(ConvidarUsuarioComando request, CancellationToken ct)
        => servico.ConvidarUsuarioAsync(new(request.Nome, request.Email, request.Perfil), ct);
}
