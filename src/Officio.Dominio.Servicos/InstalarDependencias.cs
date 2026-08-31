using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Modulos;
using Officio.Dominio.Servicos.Servicos;

namespace Officio.Dominio.Servicos;

public static class InstalarDependencias
{
    public static IServiceCollection AdicionarServicosDominio(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        InstalarDependenciasServicos
            .AdicionarServicos(servicos, configuracao);
        servicos.AddScoped<IEstimadorQualidadeDocumento, EstimadorQualidadeDocumento>();
        servicos.AddScoped<IClienteServico, ClienteServico>();

        return servicos;
    }
}
