using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Identidade;

public class ServicoEmpresaAtual : IEmpresaAtual
{
    public Guid TenantId { get; private set; }
    public void SetTenant(Guid tenantId) => TenantId = tenantId;
}





