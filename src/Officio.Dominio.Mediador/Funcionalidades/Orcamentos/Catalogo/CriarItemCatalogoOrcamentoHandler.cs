using MediatR;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class CriarItemCatalogoOrcamentoHandler(
    IOrcamentoRepositorio repositorio,
    IEmpresaAtual empresaAtual) : IRequestHandler<CriarItemCatalogoOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(CriarItemCatalogoOrcamentoComando request, CancellationToken ct)
    {
        var item = ItemCatalogoOrcamento.Criar(
            empresaAtual.TenantId,
            request.Descricao,
            request.Secao,
            request.Tipo,
            request.ValorReferencia);

        await repositorio.AdicionarItemCatalogoAsync(item, ct);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(item.Id);
    }
}
