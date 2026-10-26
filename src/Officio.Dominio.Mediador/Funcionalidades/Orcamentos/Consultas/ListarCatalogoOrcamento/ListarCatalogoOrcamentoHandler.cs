using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ListarCatalogoOrcamento;

public sealed class ListarCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<ListarCatalogoOrcamentoConsulta, Resultado<List<CatalogoOrcamentoDto>>>
{
    public Task<Resultado<List<CatalogoOrcamentoDto>>> Handle(ListarCatalogoOrcamentoConsulta request, CancellationToken ct)
        => orcamentoServico.ListarCatalogoAsync(request.IncluirInativos, ct);
}
