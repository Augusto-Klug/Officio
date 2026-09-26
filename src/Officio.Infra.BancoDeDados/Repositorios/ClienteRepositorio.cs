using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Comum;
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
            .Include(cliente => cliente.Empresa)
                .ThenInclude(empresa => empresa.Endereco)
                .ThenInclude(endereco => endereco!.Municipio)
                    .ThenInclude(municipio => municipio.Estado)
            .Include(cliente => cliente.Socios)
            .Include(cliente => cliente.Contratos)
            .Include(cliente => cliente.Documentos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var documento = DocumentoFiscal.SomenteDigitos(termo);
            consulta = consulta.Where(cliente =>
                cliente.Empresa.RazaoSocial.Contains(termo) ||
                (cliente.Empresa.NomeFantasia != null && cliente.Empresa.NomeFantasia.Contains(termo)) ||
                (cliente.Empresa.Cnpj.Contains(termo) ||
                 (documento != null && cliente.Empresa.Cnpj
                         .Replace(".", "")
                         .Replace("/", "")
                         .Replace("-", "")
                         .Contains(documento))) ||
                (cliente.SciCodigo != null && cliente.SciCodigo.Contains(termo)));
        }

        if (ativo.HasValue)
            consulta = consulta.Where(cliente => cliente.Ativo == ativo.Value);

        return consulta
            .OrderBy(cliente => cliente.Empresa.RazaoSocial)
            .ToListAsync(cancellationToken);
    }

    public Task<Cliente?> ObterPorIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        => Contexto.Clients
            .Include(cliente => cliente.Empresa)
            .FirstOrDefaultAsync(cliente => cliente.Id == clienteId, cancellationToken);

    public Task<Cliente?> ObterDetalheAsync(Guid clienteId, CancellationToken cancellationToken = default)
        => Contexto.Clients
            .AsNoTracking()
            .Include(cliente => cliente.Empresa)
                .ThenInclude(empresa => empresa.Endereco)
                .ThenInclude(endereco => endereco!.Municipio)
                    .ThenInclude(municipio => municipio.Estado)
            .Include(cliente => cliente.Socios)
            .Include(cliente => cliente.Contratos)
            .Include(cliente => cliente.Documentos)
            .FirstOrDefaultAsync(cliente => cliente.Id == clienteId, cancellationToken);
}
