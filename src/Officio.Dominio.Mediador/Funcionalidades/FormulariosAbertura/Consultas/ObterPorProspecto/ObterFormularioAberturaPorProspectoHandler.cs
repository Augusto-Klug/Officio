using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.FormulariosAbertura.Consultas.ObterPorProspecto;

public class ObterFormularioAberturaPorProspectoHandler(IFormularioAberturaServico servico)
    : IRequestHandler<ObterFormularioAberturaPorProspectoConsulta, Resultado<FormularioAberturaDto>>
{
    public Task<Resultado<FormularioAberturaDto>> Handle(ObterFormularioAberturaPorProspectoConsulta request, CancellationToken ct)
        => servico.ObterPorProspectoAsync(request.ProspectoId, ct);
}
