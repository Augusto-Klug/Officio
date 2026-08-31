namespace Officio.Dominio.Interfaces;

public interface IServicoPdf
{
    Task<Stream> GerarContratoAsync(DadosPdfContrato data, CancellationToken ct = default);
}

public record DadosPdfContrato
{
    public required string NumeroContrato { get; init; }
    public required DateTime DataEmissao { get; init; }
    public required int Versao { get; init; }

    public required DadosContratada Contratada { get; init; }
    public required DadosContratante Contratante { get; init; }
    public required IReadOnlyList<DadosSocio> Socios { get; init; }
    public required IReadOnlyList<DadosServico> Servicos { get; init; }

    public required decimal ValorMensalidade { get; init; }
    public required decimal ValorPorFuncionario { get; init; }
    public required int QtdFuncionarios { get; init; }
    public required decimal ValorTaxasExtras { get; init; }
    public required decimal Desconto { get; init; }
    public required decimal ValorTotal { get; init; }
    public required int DiaVencimento { get; init; }

    public required DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public required bool RenovacaoAuto { get; init; }
    public string? ClausulasExtras { get; init; }
}

public record DadosContratada(
    string RazaoSocial, string Cnpj,
    string Endereco, string Cidade, string Uf,
    string Email, string Telefone,
    string? LogoBase64 = null);

public record DadosContratante(
    string RazaoSocial, string NomeFantasia,
    string Cnpj, string RegimeTributario,
    string Endereco, string Cidade, string Uf,
    string Email, string Telefone);

public record DadosSocio(
    string Nome, string Cpf,
    decimal Participacao, bool Administrador);

public record DadosServico(
    string Descricao, string Periodicidade,
    decimal Valor);





