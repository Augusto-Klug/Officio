using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;

namespace Officio.Dominio.Servicos;

public static class InstalarDependencias
{
    public static IServiceCollection AdicionarServicosDominio(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        Microsoft.Extensions.DependencyInjection.InstalarDependenciasServicos
            .AdicionarServicos(servicos, configuracao);
        servicos.AddScoped<IEstimadorQualidadeDocumento, EstimadorQualidadeDocumento>();

        return servicos;
    }
}
