using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Servicos.Interfaces;
using Officio.Dominio.Servicos.Modelos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public class CriarProspectoManipulador(
    IProspectoServico prospectoServico) : IRequestHandler<CriarProspectoComando, Resultado<ProspectoCriadoDto>>
{
    public async Task<Resultado<ProspectoCriadoDto>> Handle(CriarProspectoComando request, CancellationToken ct)
    {
        var resultado = await prospectoServico.CriarAsync(
            new CriarProspectoRequisicao(
                request.RazaoSocial,
                request.Cnpj,
                request.RegimeTributario,
                request.Socios?.Select(s => new CriarSocioRequisicao(s.Nome, s.Cpf, s.Participacao)).ToList()),
            ct);

        return resultado.Sucesso
            ? Resultado<ProspectoCriadoDto>.CriarSucesso(
                new ProspectoCriadoDto(resultado.Valor!.Id, resultado.Valor.RazaoSocial))
            : Resultado<ProspectoCriadoDto>.CriarFalha(resultado.Erro!);
    }
}
