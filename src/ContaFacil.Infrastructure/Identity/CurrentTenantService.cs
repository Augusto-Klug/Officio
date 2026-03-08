using ContaFacil.Application.Common.Interfaces;

namespace ContaFacil.Infrastructure.Identity;

public class CurrentTenantService : ICurrentTenant
{
    public Guid TenantId { get; private set; }
    public void SetTenant(Guid tenantId) => TenantId = tenantId;
}
