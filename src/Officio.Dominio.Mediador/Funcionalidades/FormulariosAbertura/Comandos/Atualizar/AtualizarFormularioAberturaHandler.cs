using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.FormulariosAbertura.Comandos.Atualizar;

public class AtualizarFormularioAberturaHandler(IFormularioAberturaServico servico)
    : IRequestHandler<AtualizarFormularioAberturaComando, Resultado<FormularioAberturaDto>>
{
    public Task<Resultado<FormularioAberturaDto>> Handle(AtualizarFormularioAberturaComando request, CancellationToken ct)
        => servico.AtualizarAsync(request.Requisicao, ct);
}
