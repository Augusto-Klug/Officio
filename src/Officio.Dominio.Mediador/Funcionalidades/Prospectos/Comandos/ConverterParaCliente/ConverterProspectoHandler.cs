using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;

internal class ConverterProspectoHandler(
    IProspectoServico prospectoServico) : IRequestHandler<ConverterProspectoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(
        ConverterProspectoComando request,
        CancellationToken ct)
        => await prospectoServico.ConverterParaClienteAsync(request.ProspectId, ct);
}
