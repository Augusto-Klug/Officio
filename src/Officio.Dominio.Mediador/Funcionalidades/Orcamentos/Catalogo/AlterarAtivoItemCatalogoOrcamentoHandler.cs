using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class AlterarAtivoItemCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AlterarAtivoItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(AlterarAtivoItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new AlterarAtivoItemCatalogoOrcamentoRequisicao(request.ItemId, request.Ativo);

        return await orcamentoServico.AlterarAtivoItemCatalogoAsync(requisicao, ct);
    }
}
