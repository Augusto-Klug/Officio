using MediatR;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.SolicitarRedefinicaoSenha;

public class SolicitarRedefinicaoSenhaHandler(IGestaoAcessoServico servico)
    : IRequestHandler<SolicitarRedefinicaoSenhaComando>
{
    public async Task Handle(SolicitarRedefinicaoSenhaComando request, CancellationToken ct)
        => await servico.SolicitarRedefinicaoSenhaAsync(request.Email, ct);
}
