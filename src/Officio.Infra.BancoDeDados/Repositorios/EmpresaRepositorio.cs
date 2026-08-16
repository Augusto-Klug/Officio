using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class EmpresaRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IEmpresaRepositorio
{
    public async Task<Empresa?> ObterPorIdAsync(Guid empresaId, CancellationToken cancellationToken = default)
        => await ObterPorIdAsync<Empresa>(empresaId, cancellationToken);
}
