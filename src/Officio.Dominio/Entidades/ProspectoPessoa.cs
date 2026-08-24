using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class ProspectoPessoa : EntidadeBase
{
    public Guid ProspectId { get; private set; }
    public Guid PessoaId { get; private set; }
    public Guid TipoRelacaoId { get; private set; }
    public bool Principal { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public Prospecto Prospecto { get; private set; } = default!;
    public Pessoa Pessoa { get; private set; } = default!;
    public TipoRelacao TipoRelacao { get; private set; } = default!;

    private ProspectoPessoa() { }

    public static ProspectoPessoa Criar(Guid prospectId, Guid pessoaId, Guid tipoRelacaoId, bool principal, string? observacao = null)
        => new()
        {
            ProspectId = prospectId,
            PessoaId = pessoaId,
            TipoRelacaoId = tipoRelacaoId,
            Principal = principal,
            Observacao = observacao
        };
}
