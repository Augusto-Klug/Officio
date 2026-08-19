using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class AlterarAtivoItemCatalogoOrcamentoHandler(IOrcamentoRepositorio repositorio)
    : IRequestHandler<AlterarAtivoItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(AlterarAtivoItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var item = await repositorio.ObterItemCatalogoAsync(request.ItemId, ct);
        if (item is null)
            return Resultado<Guid>.CriarFalha("Item de catalogo nao encontrado.");

        if (request.Ativo)
            item.Ativar();
        else
            item.Desativar();

        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(item.Id);
    }
}
