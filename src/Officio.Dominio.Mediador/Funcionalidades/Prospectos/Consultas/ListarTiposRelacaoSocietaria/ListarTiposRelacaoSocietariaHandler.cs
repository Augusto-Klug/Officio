using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ListarTiposRelacaoSocietaria;

public sealed class ListarTiposRelacaoSocietariaHandler(IProspectoServico servico)
    : IRequestHandler<ListarTiposRelacaoSocietariaConsulta, List<TipoRelacaoSocietariaDto>>
{
    public Task<List<TipoRelacaoSocietariaDto>> Handle(ListarTiposRelacaoSocietariaConsulta request, CancellationToken ct)
        => servico.ListarTiposRelacaoSocietariaAsync(ct);
}
