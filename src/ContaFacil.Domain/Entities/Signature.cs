using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Entities;

public class Signature : AuditableEntity
{
    public Guid ContractId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public string Tipo { get; private set; } = default!; // cliente | contador
    public bool Assinado { get; private set; }
    public DateTime? AssinadoEm { get; private set; }
    public string? ArquivoManualPath { get; private set; }

    private Signature() { }

    public static Signature Create(Guid tenantId, Guid contractId, string nome, string email, string? cpf, string tipo)
    {
        return new Signature
        {
            TenantId = tenantId,
            ContractId = contractId,
            Nome = nome,
            Email = email,
            Cpf = cpf,
            Tipo = tipo
        };
    }

    public void MarkAsSigned()
    {
        Assinado = true;
        AssinadoEm = DateTime.UtcNow;
    }

    public void SetManualUpload(string path)
    {
        ArquivoManualPath = path;
        MarkAsSigned();
    }
}
