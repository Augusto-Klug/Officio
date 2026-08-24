using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class AssinaturaSignatario : EntidadeAuditavel
{
    public Guid AssinaturaSolicitacaoId { get; private set; }
    public Guid? PessoaId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public string Papel { get; private set; } = default!;
    public int Ordem { get; private set; }
    public DateTime? AssinadoEm { get; private set; }
    public AssinaturaSolicitacao AssinaturaSolicitacao { get; private set; } = default!;
    public Pessoa? Pessoa { get; private set; }

    private AssinaturaSignatario() { }

    public static AssinaturaSignatario Criar(Guid tenantId, Guid assinaturaSolicitacaoId, string nome, string email, string papel, int ordem)
        => new() { TenantId = tenantId, AssinaturaSolicitacaoId = assinaturaSolicitacaoId, Nome = nome, Email = email, Papel = papel, Ordem = ordem };
}
