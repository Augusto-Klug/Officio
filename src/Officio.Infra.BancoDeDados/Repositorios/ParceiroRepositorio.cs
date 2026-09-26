using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ParceiroRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IParceiroRepositorio
{
    public Task<Parceiro?> ObterPorIdAsync(Guid parceiroId, CancellationToken cancellationToken = default)
        => ObterPorIdAsync<Parceiro>(parceiroId, cancellationToken);
}
