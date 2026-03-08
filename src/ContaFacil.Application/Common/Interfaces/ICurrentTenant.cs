namespace ContaFacil.Application.Common.Interfaces;

public interface ICurrentTenant
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}
