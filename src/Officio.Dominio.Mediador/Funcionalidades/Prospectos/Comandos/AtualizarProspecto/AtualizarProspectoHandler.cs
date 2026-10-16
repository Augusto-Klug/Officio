using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarProspecto;

internal class AtualizarProspectoHandler(IProspectoServico servico)
    : IRequestHandler<AtualizarProspectoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AtualizarProspectoComando request, CancellationToken ct)
        => servico.AtualizarAsync(request.Id, request.Requisicao, ct);
}
