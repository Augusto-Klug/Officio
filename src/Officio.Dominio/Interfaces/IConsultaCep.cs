namespace Officio.Dominio.Interfaces;

public interface IConsultaCep
{
    Task<EnderecoCep?> ConsultarAsync(
        string cep,
        CancellationToken cancellationToken = default);
}

public sealed record EnderecoCep(
    string Cep,
    string? Logradouro,
    string? Complemento,
    string? Bairro,
    string Cidade,
    string Uf,
    string CodigoMunicipioIbge);
