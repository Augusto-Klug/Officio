using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class EmpresaPessoa : EntidadeBase
{
    public Guid EmpresaId { get; private set; }
    public Guid PessoaId { get; private set; }
    public Guid TipoRelacaoId { get; private set; }
    public string? Cargo { get; private set; }
    public decimal? PercentualParticipacao { get; private set; }
    public bool? Administrador { get; private set; }
    public bool Principal { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public Empresa Empresa { get; private set; } = default!;
    public Pessoa Pessoa { get; private set; } = default!;
    public TipoRelacao TipoRelacao { get; private set; } = default!;

    private EmpresaPessoa() { }

    public static EmpresaPessoa Criar(Guid empresaId, Guid pessoaId, Guid tipoRelacaoId, bool principal = false)
        => new() { EmpresaId = empresaId, PessoaId = pessoaId, TipoRelacaoId = tipoRelacaoId, Principal = principal };
}
