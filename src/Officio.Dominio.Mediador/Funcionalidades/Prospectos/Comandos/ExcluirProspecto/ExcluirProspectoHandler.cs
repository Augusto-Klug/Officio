using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ExcluirProspecto;

internal class ExcluirProspectoHandler(IProspectoServico servico)
    : IRequestHandler<ExcluirProspectoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(ExcluirProspectoComando request, CancellationToken ct)
        => servico.ExcluirAsync(request.Id, ct);
}
