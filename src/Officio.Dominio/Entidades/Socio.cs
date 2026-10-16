using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Socio : EntidadeAuditavel
{
    public string Nome { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public decimal Participacao { get; private set; }
    public Guid? ProspectId { get; private set; }
    public Guid? ClientId { get; private set; }

    private Socio() { }

    public static Socio Create(Guid tenantId, string nome, string? cpf, decimal participacao, Guid? prospectId = null, Guid? clientId = null)
    {
        return new Socio
        {
            TenantId = tenantId,
            Nome = nome,
            Cpf = DocumentoFiscal.SomenteDigitos(cpf),
            Participacao = participacao,
            ProspectId = prospectId,
            ClientId = clientId
        };
    }

    public static Socio CopyFor(Socio source, Guid clientId)
    {
        return new Socio
        {
            TenantId = source.TenantId,
            Nome = source.Nome,
            Cpf = source.Cpf,
            Participacao = source.Participacao,
            ClientId = clientId
        };
    }

    public void AtualizarDados(string nome, string? cpf, decimal participacao)
    {
        Nome = nome.Trim();
        Cpf = DocumentoFiscal.SomenteDigitos(cpf);
        Participacao = participacao;
    }
}





