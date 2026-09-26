using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal sealed class LocalidadeRepositorio(ContextoAplicacao contexto) : ILocalidadeRepositorio
{
    public Task<List<Estado>> ListarEstadosAsync(CancellationToken cancellationToken = default)
        => contexto.Estados.Where(estado => estado.Ativo).ToListAsync(cancellationToken);

    public Task<Estado?> ObterEstadoPorUfAsync(string uf, CancellationToken cancellationToken = default)
        => contexto.Estados.FirstOrDefaultAsync(estado => estado.Sigla == uf, cancellationToken);

    public Task<Municipio?> ObterMunicipioPorCodigoIbgeAsync(
        string codigoIbge,
        CancellationToken cancellationToken = default)
        => contexto.Municipios.FirstOrDefaultAsync(
            municipio => municipio.CodigoIbge == codigoIbge,
            cancellationToken);

    public Task<List<Municipio>> ListarMunicipiosPorEstadoAsync(
        Guid estadoId,
        CancellationToken cancellationToken = default)
        => contexto.Municipios
            .Where(municipio => municipio.EstadoId == estadoId && municipio.Ativo)
            .ToListAsync(cancellationToken);

    public async Task AdicionarEstadoAsync(Estado estado, CancellationToken cancellationToken = default)
        => await contexto.Estados.AddAsync(estado, cancellationToken);

    public async Task AdicionarMunicipioAsync(Municipio municipio, CancellationToken cancellationToken = default)
        => await contexto.Municipios.AddAsync(municipio, cancellationToken);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => await contexto.SaveChangesAsync(cancellationToken);
}
