using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Consultas.ObterDadosContabilidade;

public sealed class ObterDadosContabilidadeHandler(IContabilidadeServico servico)
    : IRequestHandler<ObterDadosContabilidadeConsulta, Resultado<DadosContabilidadeDto>>
{
    public Task<Resultado<DadosContabilidadeDto>> Handle(ObterDadosContabilidadeConsulta request, CancellationToken ct)
        => servico.ObterAsync(ct);
}
