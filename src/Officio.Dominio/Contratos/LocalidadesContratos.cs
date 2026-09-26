namespace Officio.Dominio.Contratos;

public sealed record EstadoLocalidade(string CodigoIbge, string Sigla, string Nome);

public sealed record MunicipioLocalidade(string CodigoIbge, string Nome, string Uf);

public sealed record EnderecoCepLocalidade(
    string Cep,
    string? Logradouro,
    string? Complemento,
    string? Bairro,
    string Cidade,
    string Uf,
    string MunicipioCodigoIbge);
