using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class ListarCatalogoOrcamentoHandler(IOrcamentoRepositorio repositorio)
    : IRequestHandler<ListarCatalogoOrcamentoConsulta, Resultado<List<CatalogoOrcamentoDto>>>
{
    public async Task<Resultado<List<CatalogoOrcamentoDto>>> Handle(
        ListarCatalogoOrcamentoConsulta request,
        CancellationToken ct)
    {
        var itens = await repositorio.ObterCatalogoAtivoAsync(ct);
        var catalogo = itens
            .Select(item => new CatalogoOrcamentoDto(
                item.Id,
                item.Descricao,
                item.Secao.ToString(),
                item.Tipo.ToString(),
                item.ValorReferencia,
                item.Ativo))
            .ToList();

        return Resultado<List<CatalogoOrcamentoDto>>.CriarSucesso(catalogo);
    }
}
