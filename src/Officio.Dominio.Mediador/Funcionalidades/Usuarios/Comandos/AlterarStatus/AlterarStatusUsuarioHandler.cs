using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.AlterarStatus;

public class AlterarStatusUsuarioHandler(IGestaoAcessoServico servico)
    : IRequestHandler<AlterarStatusUsuarioComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AlterarStatusUsuarioComando request, CancellationToken ct)
        => servico.AlterarStatusAsync(request.UsuarioId, request.Ativo, ct);
}
