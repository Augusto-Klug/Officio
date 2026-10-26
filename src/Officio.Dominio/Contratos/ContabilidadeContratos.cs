namespace Officio.Dominio.Contratos;

public record EnderecoContabilidadeDto(string? Cep, string Logradouro, string? Numero, string? Complemento,
    string? Bairro, string Cidade, string Uf, string MunicipioCodigoIbge);

public record ResponsavelTecnicoDto(string Nome, string Cpf, string Profissao, string Crc, EnderecoContabilidadeDto? Endereco);

public record DadosContabilidadeDto(string RazaoSocial, string? NomeFantasia, string Cnpj, string? Crc,
    string? Email, string? Telefone, EnderecoContabilidadeDto? Endereco, ResponsavelTecnicoDto? ResponsavelTecnico);
