using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

internal class CriarProspectoHandler(
    IProspectoServico prospectoServico) : IRequestHandler<CriarProspectoComando, Resultado<ProspectoCriadoDto>>
{
    public async Task<Resultado<ProspectoCriadoDto>> Handle(CriarProspectoComando request, CancellationToken ct)
    {
        var resultado = await prospectoServico.CriarAsync(request.Requisicao, ct);

        return resultado.Sucesso
            ? Resultado<ProspectoCriadoDto>.CriarSucesso(
                new ProspectoCriadoDto(resultado.Valor!.Id, resultado.Valor.RazaoSocial))
            : Resultado<ProspectoCriadoDto>.CriarFalha(resultado.Erro!);
    }
}
