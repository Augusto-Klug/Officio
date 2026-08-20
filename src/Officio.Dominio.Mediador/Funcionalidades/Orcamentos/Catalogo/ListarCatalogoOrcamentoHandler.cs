using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class ListarCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<ListarCatalogoOrcamentoConsulta, Resultado<List<CatalogoOrcamentoDto>>>
{
    public async Task<Resultado<List<CatalogoOrcamentoDto>>> Handle(
        ListarCatalogoOrcamentoConsulta request,
        CancellationToken ct)
    {
        var resultado = await orcamentoServico.ListarCatalogoAsync(ct);

        return resultado.ParaResultado(itens => itens
            .Select(item => new CatalogoOrcamentoDto(
                item.Id,
                item.Descricao,
                item.Secao,
                item.Tipo,
                item.ValorReferencia,
                item.Ativo))
            .ToList());
    }
}
