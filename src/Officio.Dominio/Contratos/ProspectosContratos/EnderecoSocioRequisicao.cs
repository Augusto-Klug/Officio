namespace Officio.Dominio.Contratos;

public record EnderecoSocioRequisicao(
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Complemento = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Uf = null,
    string? MunicipioCodigoIbge = null);
