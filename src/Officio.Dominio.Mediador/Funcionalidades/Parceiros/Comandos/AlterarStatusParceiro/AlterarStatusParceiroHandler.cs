using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.AlterarStatusParceiro;

public class AlterarStatusParceiroHandler(IGestaoAcessoServico servico)
    : IRequestHandler<AlterarStatusParceiroComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AlterarStatusParceiroComando request, CancellationToken ct)
        => servico.AlterarStatusParceiroAsync(request.ParceiroId, request.Ativo, ct);
}
