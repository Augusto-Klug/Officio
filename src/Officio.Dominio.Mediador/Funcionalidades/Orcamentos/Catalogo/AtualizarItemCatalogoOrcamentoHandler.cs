using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class AtualizarItemCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AtualizarItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(AtualizarItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new AtualizarItemCatalogoOrcamentoRequisicao(
            request.ItemId,
            request.Descricao,
            request.Secao,
            request.Tipo,
            request.ValorReferencia);

        return (await orcamentoServico.AtualizarItemCatalogoAsync(requisicao, ct)).ParaResultado();
    }
}
