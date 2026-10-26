using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarAtivoItemCatalogoOrcamento;

public sealed class AlterarAtivoItemCatalogoOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AlterarAtivoItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public Task<Resultado<Guid>> Handle(AlterarAtivoItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new AlterarAtivoItemCatalogoOrcamentoRequisicao(request.ItemId, request.Ativo);
        return orcamentoServico.AlterarAtivoItemCatalogoAsync(requisicao, ct);
    }
}
