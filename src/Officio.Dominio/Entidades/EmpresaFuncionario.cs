using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class EmpresaFuncionario : EntidadeBase
{
    public Guid EmpresaId { get; private set; }
    public Guid AdmissaoOrigemId { get; private set; }
    public Guid? PessoaId { get; private set; }
    public string NomeFuncionario { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public DateOnly DataAdmissao { get; private set; }
    public DateOnly? DataDesligamento { get; private set; }
    public string? Funcao { get; private set; }
    public decimal? Salario { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Empresa Empresa { get; private set; } = default!;
    public Admissao AdmissaoOrigem { get; private set; } = default!;
    public Pessoa? Pessoa { get; private set; }

    private EmpresaFuncionario() { }

    public static EmpresaFuncionario Criar(
        Guid empresaId,
        Guid admissaoOrigemId,
        string nomeFuncionario,
        DateOnly dataAdmissao,
        string? cpf = null,
        Guid? pessoaId = null,
        string? funcao = null,
        decimal? salario = null)
        => new()
        {
            EmpresaId = empresaId,
            AdmissaoOrigemId = admissaoOrigemId,
            PessoaId = pessoaId,
            NomeFuncionario = nomeFuncionario.Trim(),
            Cpf = DocumentoFiscal.SomenteDigitos(cpf),
            DataAdmissao = dataAdmissao,
            Funcao = string.IsNullOrWhiteSpace(funcao) ? null : funcao.Trim(),
            Salario = salario
        };
}
