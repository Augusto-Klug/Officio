using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;

namespace Officio.Dominio.Servicos.Modulos;

public static class InstalarDependenciasServicos
{
    public static IServiceCollection AdicionarServicos(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        servicos.TryAddScoped<IProspectoServico, ProspectoServico>();
        servicos.TryAddScoped<IClienteServico, ClienteServico>();
        servicos.TryAddScoped<IDocumentoServico, DocumentoServico>();
        servicos.TryAddScoped<IContratoServico, ContratoServico>();
        servicos.TryAddScoped<IOrcamentoServico, OrcamentoServico>();
        servicos.TryAddScoped<INotificacaoServico, NotificacaoServico>();
        servicos.TryAddScoped<IEstimadorQualidadeDocumento, EstimadorQualidadeDocumento>();

        return servicos;
    }
}
