namespace Officio.Dominio.Servicos.Interfaces;

public interface IEmpresaAtual
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}





