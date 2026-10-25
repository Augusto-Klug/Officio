using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas;
public record ListarTiposRelacaoSocietariaConsulta : IRequest<List<TipoRelacaoSocietariaDto>>;
public class ListarTiposRelacaoSocietariaHandler(IProspectoServico servico)
    : IRequestHandler<ListarTiposRelacaoSocietariaConsulta, List<TipoRelacaoSocietariaDto>>
{
    public async Task<List<TipoRelacaoSocietariaDto>> Handle(ListarTiposRelacaoSocietariaConsulta request, CancellationToken ct)
        => await servico.ListarTiposRelacaoSocietariaAsync(ct);
}
