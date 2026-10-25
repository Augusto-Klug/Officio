using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade;

public record ObterDadosContabilidadeConsulta : IRequest<Resultado<DadosContabilidadeDto>>;
public record AtualizarDadosContabilidadeComando(DadosContabilidadeDto Dados) : IRequest<Resultado<DadosContabilidadeDto>>;

public sealed class ObterDadosContabilidadeHandler(IContabilidadeServico servico)
    : IRequestHandler<ObterDadosContabilidadeConsulta, Resultado<DadosContabilidadeDto>>
{
    public Task<Resultado<DadosContabilidadeDto>> Handle(ObterDadosContabilidadeConsulta request, CancellationToken ct)
        => servico.ObterAsync(ct);
}

public sealed class AtualizarDadosContabilidadeHandler(IContabilidadeServico servico)
    : IRequestHandler<AtualizarDadosContabilidadeComando, Resultado<DadosContabilidadeDto>>
{
    public Task<Resultado<DadosContabilidadeDto>> Handle(AtualizarDadosContabilidadeComando request, CancellationToken ct)
        => servico.AtualizarAsync(request.Dados, ct);
}
