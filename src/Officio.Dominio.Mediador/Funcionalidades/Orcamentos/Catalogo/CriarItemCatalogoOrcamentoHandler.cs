using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class CriarItemCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<CriarItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(CriarItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new CriarItemCatalogoOrcamentoRequisicao(
            request.Descricao,
            request.Secao,
            request.Tipo,
            request.ValorReferencia,
            request.NaturezaCobranca);

        return await orcamentoServico.CriarItemCatalogoAsync(requisicao, ct);
    }
}
