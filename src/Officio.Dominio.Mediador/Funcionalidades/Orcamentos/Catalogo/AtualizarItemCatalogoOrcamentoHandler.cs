using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class AtualizarItemCatalogoOrcamentoHandler(IOrcamentoRepositorio repositorio)
    : IRequestHandler<AtualizarItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(AtualizarItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var item = await repositorio.ObterItemCatalogoAsync(request.ItemId, ct);
        if (item is null)
            return Resultado<Guid>.CriarFalha("Item de catalogo nao encontrado.");

        item.Atualizar(request.Descricao, request.Secao, request.Tipo, request.ValorReferencia);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(item.Id);
    }
}
