using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IContabilidadeRepositorio
{
    Task<Parceiro?> ObterAtualAsync(CancellationToken ct = default);
    Task<bool> CnpjEmUsoAsync(string cnpj, CancellationToken ct = default);
    Task AdicionarResponsavelAsync(ResponsavelTecnico responsavel, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
