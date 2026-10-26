using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Comandos.AtualizarDadosContabilidade;

public sealed class AtualizarDadosContabilidadeHandler(IContabilidadeServico servico)
    : IRequestHandler<AtualizarDadosContabilidadeComando, Resultado<DadosContabilidadeDto>>
{
    public Task<Resultado<DadosContabilidadeDto>> Handle(AtualizarDadosContabilidadeComando request, CancellationToken ct)
        => servico.AtualizarAsync(request.Dados, ct);
}
