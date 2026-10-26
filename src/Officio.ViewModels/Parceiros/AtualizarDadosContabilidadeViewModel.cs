namespace Officio.ViewModels.Parceiros;

public record AtualizarDadosContabilidadeViewModel(string RazaoSocial, string? NomeFantasia, string Cnpj,
    string? Crc, string? Email, string? Telefone, EnderecoContabilidadeViewModel? Endereco, ResponsavelTecnicoViewModel? ResponsavelTecnico);

public record EnderecoContabilidadeViewModel(string? Cep, string Logradouro, string? Numero, string? Complemento,
    string? Bairro, string Cidade, string Uf, string MunicipioCodigoIbge);

public record ResponsavelTecnicoViewModel(string Nome, string Cpf, string Profissao, string Crc, EnderecoContabilidadeViewModel? Endereco);
