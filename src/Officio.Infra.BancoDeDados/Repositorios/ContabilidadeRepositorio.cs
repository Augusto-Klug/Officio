using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal sealed class ContabilidadeRepositorio(ContextoAplicacao contexto, IEmpresaAtual tenant) : IContabilidadeRepositorio
{
    public Task<Parceiro?> ObterAtualAsync(CancellationToken ct = default)
        => contexto.Parceiros
            .Include(p => p.Endereco).ThenInclude(e => e!.Municipio).ThenInclude(m => m.Estado)
            .Include(p => p.ResponsavelTecnico).ThenInclude(r => r!.Endereco)
                .ThenInclude(e => e!.Municipio).ThenInclude(m => m.Estado)
            .SingleOrDefaultAsync(p => p.Id == tenant.TenantId, ct);

    public Task<bool> CnpjEmUsoAsync(string cnpj, CancellationToken ct = default)
        => contexto.Parceiros.AnyAsync(p => p.Id != tenant.TenantId && p.Cnpj == cnpj, ct);

    public Task AdicionarResponsavelAsync(ResponsavelTecnico responsavel, CancellationToken ct = default)
        => contexto.ResponsaveisTecnicos.AddAsync(responsavel, ct).AsTask();

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) => contexto.SaveChangesAsync(ct);
}
