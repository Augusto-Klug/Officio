using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;

internal class ConverterProspectoHandler(
    IProspectoServico prospectoServico) : IRequestHandler<ConverterProspectoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(
        ConverterProspectoComando request,
        CancellationToken ct)
    {
        var resultado = await prospectoServico.ConverterParaClienteAsync(request.ProspectId, ct);

        return resultado.Sucesso
            ? Resultado<Guid>.CriarSucesso(resultado.Valor)
            : Resultado<Guid>.CriarFalha(resultado.Erro!);
    }
}
