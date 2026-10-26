using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarItemCatalogoOrcamento;

public sealed class AtualizarItemCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AtualizarItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public Task<Resultado<Guid>> Handle(AtualizarItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new AtualizarItemCatalogoOrcamentoRequisicao(
            request.ItemId,
            request.Descricao,
            request.Secao,
            request.Tipo,
            request.ValorReferencia,
            request.NaturezaCobranca);

        return orcamentoServico.AtualizarItemCatalogoAsync(requisicao, ct);
    }
}
