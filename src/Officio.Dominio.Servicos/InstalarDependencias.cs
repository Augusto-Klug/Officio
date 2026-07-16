using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Officio.Dominio.Servicos;

public static class InstalarDependencias
{
    public static IServiceCollection AdicionarServicosDominio(
        this IServiceCollection servicos,
        IConfiguration configuracao)
        => Microsoft.Extensions.DependencyInjection.InstalarDependenciasServicos
            .AdicionarServicos(servicos, configuracao);
}
