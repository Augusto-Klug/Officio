using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ClienteRepositorio(ContextoAplicacao contexto)
    : RepositorioBase<ContextoAplicacao, Guid>(contexto), IClienteRepositorio
{
    public Task<List<Cliente>> ListarAsync(
        string? busca = null,
        bool? ativo = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = Contexto.Clients
            .AsNoTracking()
            .Include(cliente => cliente.Socios)
            .Include(cliente => cliente.Contratos)
            .Include(cliente => cliente.Documentos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(cliente =>
                cliente.RazaoSocial.Contains(termo) ||
                (cliente.NomeFantasia != null && cliente.NomeFantasia.Contains(termo)) ||
                (cliente.Cnpj != null && cliente.Cnpj.Contains(termo)) ||
                (cliente.Cpf != null && cliente.Cpf.Contains(termo)) ||
                (cliente.SciCodigo != null && cliente.SciCodigo.Contains(termo)));
        }

        if (ativo.HasValue)
            consulta = consulta.Where(cliente => cliente.Ativo == ativo.Value);

        return consulta
            .OrderBy(cliente => cliente.RazaoSocial)
            .ToListAsync(cancellationToken);
    }

    public Task<Cliente?> ObterPorIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        => Contexto.Clients
            .FirstOrDefaultAsync(cliente => cliente.Id == clienteId, cancellationToken);

    public Task<Cliente?> ObterDetalheAsync(Guid clienteId, CancellationToken cancellationToken = default)
        => Contexto.Clients
            .AsNoTracking()
            .Include(cliente => cliente.Socios)
            .Include(cliente => cliente.Contratos)
            .Include(cliente => cliente.Documentos)
            .FirstOrDefaultAsync(cliente => cliente.Id == clienteId, cancellationToken);
}
