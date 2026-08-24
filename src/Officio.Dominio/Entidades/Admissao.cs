using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Admissao : EntidadeAuditavel
{
    private readonly List<AdmissaoDocumento> _documentos = [];
    private readonly List<AdmissaoDependente> _dependentes = [];

    public Guid EmpresaId { get; private set; }
    public Guid? PessoaId { get; private set; }
    public Guid StatusAdmissaoId { get; private set; }
    public string NomeFuncionario { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public string? Rg { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? Sexo { get; private set; }
    public string? EstadoCivil { get; private set; }
    public string? Escolaridade { get; private set; }
    public Guid? EnderecoId { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Pis { get; private set; }
    public string? Banco { get; private set; }
    public string? Agencia { get; private set; }
    public string? Conta { get; private set; }
    public string? TipoConta { get; private set; }
    public DateOnly? DataAdmissaoPrevista { get; private set; }
    public decimal? Salario { get; private set; }
    public string? Comissao { get; private set; }
    public string? Funcao { get; private set; }
    public int? ContratoExperienciaDias { get; private set; }
    public bool? UtilizaValeTransporte { get; private set; }
    public string? HorarioTrabalho { get; private set; }
    public string? TokenAcessoHash { get; private set; }
    public DateTime? TokenExpiraEm { get; private set; }
    public DateTime? PreenchimentoFuncionarioConcluidoEm { get; private set; }
    public DateTime? PreenchimentoEmpresaConcluidoEm { get; private set; }
    public string? Observacoes { get; private set; }
    public Empresa Empresa { get; private set; } = default!;
    public Pessoa? Pessoa { get; private set; }
    public StatusAdmissao StatusAdmissao { get; private set; } = default!;
    public Endereco? Endereco { get; private set; }
    public IReadOnlyCollection<AdmissaoDocumento> Documentos => _documentos.AsReadOnly();
    public IReadOnlyCollection<AdmissaoDependente> Dependentes => _dependentes.AsReadOnly();

    private Admissao() { }

    public static Admissao Criar(Guid tenantId, Guid empresaId, Guid statusAdmissaoId, string nomeFuncionario)
        => new() { TenantId = tenantId, EmpresaId = empresaId, StatusAdmissaoId = statusAdmissaoId, NomeFuncionario = nomeFuncionario };
}
