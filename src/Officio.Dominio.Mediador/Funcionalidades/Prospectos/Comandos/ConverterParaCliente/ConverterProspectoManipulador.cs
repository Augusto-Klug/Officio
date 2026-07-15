using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Servicos.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;

public class ConverterProspectoManipulador(
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
