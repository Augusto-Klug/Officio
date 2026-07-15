using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Servicos.Interfaces;
using Officio.Dominio.Servicos.Servicos;

namespace Officio.Dominio.Servicos;

public static class InstalarDependencias
{
    public static IServiceCollection AdicionarServicosDominio(this IServiceCollection servicos)
    {
        servicos.AddScoped<IProspectoServico, ProspectoServico>();
        servicos.AddScoped<IDocumentoServico, DocumentoServico>();
        servicos.AddScoped<IContratoServico, ContratoServico>();

        return servicos;
    }
}
