namespace Officio.Dominio.Interfaces;

public interface IServicoPdf
{
    Task<Stream> GerarContratoAsync(DadosPdfContrato data, CancellationToken ct = default);
}

public record DadosPdfContrato
{
    public required string NumeroContrato { get; init; }
    public required DateTime DataEmissao { get; init; }
    public required DadosContratada Contratada { get; init; }
    public required DadosContratante Contratante { get; init; }
    public required int DiaVencimento { get; init; }

    public required DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public required int DuracaoAnos { get; init; }
    public required DateOnly CompetenciaInicialHonorarios { get; init; }
    public required decimal ValorHonorarioMensal { get; init; }
    public required decimal ValorUnitarioPorFolha { get; init; }
    public string? ClausulasExtras { get; init; }
}

public record DadosEnderecoContrato(
    string Logradouro, string Numero, string? Complemento, string Bairro,
    string Cep, string Cidade, string Uf);

public record DadosResponsavelTecnico(
    string Nome, string Profissao, string Cpf, string Crc, DadosEnderecoContrato Endereco);

public record DadosRepresentanteContrato(
    string TipoRelacao, string Nome, string Profissao, string Cpf, DadosEnderecoContrato Endereco);

public record DadosContratada(
    string RazaoSocial, string Cnpj, string Crc, DadosEnderecoContrato Endereco,
    DadosResponsavelTecnico ResponsavelTecnico);

public record DadosContratante(
    string RazaoSocial, string? NomeFantasia, string Cnpj, string? RegimeTributario,
    DadosEnderecoContrato Endereco, string? Email, string? Telefone,
    DadosRepresentanteContrato Representante);





