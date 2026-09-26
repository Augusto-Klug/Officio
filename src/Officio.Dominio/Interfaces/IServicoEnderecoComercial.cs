using Officio.Dominio.Comum;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IServicoEnderecoComercial
{
    Task<Resultado<Endereco?>> CriarAsync(
        EnderecoComercialRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<Resultado<Endereco?>> CriarDoProspectoAsync(
        Prospecto prospecto,
        CancellationToken cancellationToken = default);
}

public record EnderecoComercialRequisicao(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    Guid? MunicipioId = null);
