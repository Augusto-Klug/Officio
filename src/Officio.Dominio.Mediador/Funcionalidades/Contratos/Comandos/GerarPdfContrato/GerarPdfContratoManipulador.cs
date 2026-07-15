using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Servicos.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.GerarPdfContrato;

public class GerarPdfContratoManipulador(
    IContratoServico contratoServico) :
    IRequestHandler<GerarPdfContratoComando, Resultado<ResultadoPdfContratoDto>>
{
    public async Task<Resultado<ResultadoPdfContratoDto>> Handle(GerarPdfContratoComando request, CancellationToken ct)
    {
        var resultado = await contratoServico.GerarPdfAsync(request.ContractId, ct);

        return resultado.Sucesso
            ? Resultado<ResultadoPdfContratoDto>.CriarSucesso(
                new ResultadoPdfContratoDto(resultado.Valor!.PdfUrl, resultado.Valor.GeradoEm))
            : Resultado<ResultadoPdfContratoDto>.CriarFalha(resultado.Erro!);
    }
}
