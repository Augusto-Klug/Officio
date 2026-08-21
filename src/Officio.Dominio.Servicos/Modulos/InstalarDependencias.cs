using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;

namespace Microsoft.Extensions.DependencyInjection;

public static class InstalarDependenciasServicos
{
    public static IServiceCollection AdicionarServicos(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        servicos.TryAddScoped<IProspectoServico, ProspectoServico>();
        servicos.TryAddScoped<IDocumentoServico, DocumentoServico>();
        servicos.TryAddScoped<IContratoServico, ContratoServico>();
        servicos.TryAddScoped<IOrcamentoServico, OrcamentoServico>();

        return servicos;
    }
}
