using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarSociosProspecto;

public sealed class AtualizarSociosProspectoHandler(IProspectoServico servico)
    : IRequestHandler<AtualizarSociosProspectoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AtualizarSociosProspectoComando request, CancellationToken ct)
        => servico.AtualizarSociosAsync(request.Id, request.Dados, ct);
}
