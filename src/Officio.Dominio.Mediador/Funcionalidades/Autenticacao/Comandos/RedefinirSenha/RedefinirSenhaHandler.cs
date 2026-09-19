using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.RedefinirSenha;

public class RedefinirSenhaHandler(IGestaoAcessoServico servico)
    : IRequestHandler<RedefinirSenhaComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(RedefinirSenhaComando request, CancellationToken ct)
        => servico.RedefinirSenhaAsync(new(request.Token, request.Senha), ct);
}
