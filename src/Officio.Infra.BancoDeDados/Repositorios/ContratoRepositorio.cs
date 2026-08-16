using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ContratoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IContratoRepositorio
{
    public async Task<Contrato?> ObterParaGeracaoPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Cliente)
                .ThenInclude(cl => cl.Socios)
            .Include(c => c.Orcamento)
                .ThenInclude(o => o!.Itens)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

    public async Task<Contrato?> ObterParaAssinaturaAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

    public async Task AdicionarAssinaturaAsync(Assinatura assinatura, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Assinatura>(assinatura, cancellationToken);
}
