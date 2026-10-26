using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarItemCatalogoOrcamento;

public sealed class CriarItemCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<CriarItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public Task<Resultado<Guid>> Handle(CriarItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new CriarItemCatalogoOrcamentoRequisicao(
            request.Descricao,
            request.Secao,
            request.Tipo,
            request.ValorReferencia,
            request.NaturezaCobranca);

        return orcamentoServico.CriarItemCatalogoAsync(requisicao, ct);
    }
}
