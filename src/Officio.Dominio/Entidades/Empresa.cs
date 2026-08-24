using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Empresa : EntidadeBase
{
    public Guid? EnderecoId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Cnpj { get; private set; } = default!;
    public string RazaoSocial { get; private set; } = default!;
    public string? NomeFantasia { get; private set; }
    public string? InscricaoEstadual { get; private set; }
    public string? InscricaoMunicipal { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? TipoEmpresa { get; private set; }
    public string? RegimeTributario { get; private set; }
    public int QuantidadeFuncionariosAtual { get; private set; }
    public int QuantidadeProLaboresAtual { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Endereco? Endereco { get; private set; }
    public ICollection<EmpresaPessoa> Pessoas { get; private set; } = [];
    public ICollection<Admissao> Admissoes { get; private set; } = [];
    public ICollection<EmpresaFuncionario> Funcionarios { get; private set; } = [];

    private Empresa() { }

    public static Empresa Create(string nome, string? cnpj)
    {
        return new Empresa
        {
            Nome = nome,
            RazaoSocial = nome,
            Cnpj = cnpj ?? string.Empty
        };
    }

    public static Empresa CriarConstituida(
        string cnpj,
        string razaoSocial,
        string? nomeFantasia = null,
        int quantidadeFuncionariosAtual = 0,
        int quantidadeProLaboresAtual = 0)
    {
        return new Empresa
        {
            Nome = razaoSocial,
            Cnpj = cnpj,
            RazaoSocial = razaoSocial,
            NomeFantasia = nomeFantasia,
            QuantidadeFuncionariosAtual = quantidadeFuncionariosAtual,
            QuantidadeProLaboresAtual = quantidadeProLaboresAtual
        };
    }

    public void AtualizarQuantidadesOperacionais(int quantidadeFuncionariosAtual, int quantidadeProLaboresAtual)
    {
        QuantidadeFuncionariosAtual = quantidadeFuncionariosAtual;
        QuantidadeProLaboresAtual = quantidadeProLaboresAtual;
        AtualizadoEm = DateTime.UtcNow;
    }
}





