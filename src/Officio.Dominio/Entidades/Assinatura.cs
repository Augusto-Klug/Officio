using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Assinatura : EntidadeAuditavel
{
    public Guid ContractId { get; private set; }
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Cpf { get; private set; }
    public string Tipo { get; private set; } = default!; // cliente | contador
    public bool Assinado { get; private set; }
    public DateTime? AssinadoEm { get; private set; }
    public string? ArquivoManualPath { get; private set; }

    private Assinatura() { }

    public static Assinatura Create(Guid tenantId, Guid contractId, string nome, string email, string? cpf, string tipo)
    {
        return new Assinatura
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





