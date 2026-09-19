using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class AdmissaoDependente : EntidadeBase
{
    public Guid AdmissaoId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? Parentesco { get; private set; }
    public bool? DependenteIr { get; private set; }
    public bool? SalarioFamilia { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Admissao Admissao { get; private set; } = default!;

    private AdmissaoDependente() { }

    public static AdmissaoDependente Criar(
        Guid admissaoId,
        string nome,
        string? cpf = null,
        DateOnly? dataNascimento = null,
        string? parentesco = null,
        bool? dependenteIr = null,
        bool? salarioFamilia = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do dependente e obrigatorio.", nameof(nome));

        return new()
        {
            AdmissaoId = admissaoId,
            Nome = nome.Trim(),
            Cpf = DocumentoFiscal.SomenteDigitos(cpf),
            DataNascimento = dataNascimento,
            Parentesco = string.IsNullOrWhiteSpace(parentesco) ? null : parentesco.Trim(),
            DependenteIr = dependenteIr,
            SalarioFamilia = salarioFamilia
        };
    }
}
