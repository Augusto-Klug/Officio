using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class ListarCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<ListarCatalogoOrcamentoConsulta, Resultado<List<CatalogoOrcamentoDto>>>
{
    public async Task<Resultado<List<CatalogoOrcamentoDto>>> Handle(
        ListarCatalogoOrcamentoConsulta request,
        CancellationToken ct)
        => await orcamentoServico.ListarCatalogoAsync(ct);
}
