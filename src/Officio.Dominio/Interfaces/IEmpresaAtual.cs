namespace Officio.Dominio.Interfaces;

public interface IEmpresaAtual
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}





