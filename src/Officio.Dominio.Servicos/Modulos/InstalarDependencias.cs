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
        servicos.TryAddScoped<IContabilidadeServico, ContabilidadeServico>();
        servicos.TryAddScoped<IServicoEnderecoComercial, ServicoEnderecoComercial>();
        servicos.TryAddScoped<ILocalidadeServico, LocalidadeServico>();
        servicos.TryAddSingleton<IConsultaMunicipioIbge, ConsultaMunicipioIbgeIndisponivel>();
        servicos.TryAddSingleton<IConsultaCep, ConsultaCepIndisponivel>();
        servicos.TryAddScoped<IClienteServico, ClienteServico>();
        servicos.TryAddScoped<IDocumentoServico, DocumentoServico>();
        servicos.TryAddScoped<IContratoServico, ContratoServico>();
        servicos.TryAddScoped<IOrcamentoServico, OrcamentoServico>();
        servicos.TryAddScoped<INotificacaoServico, NotificacaoServico>();
        servicos.TryAddScoped<IGestaoAcessoServico, GestaoAcessoServico>();
        servicos.TryAddScoped<IAdmissaoServico, AdmissaoServico>();
        servicos.TryAddScoped<IAdmissaoDocumentoServico, AdmissaoDocumentoServico>();
        servicos.TryAddScoped<IFichaAdmissaoServico, FichaAdmissaoServico>();
        servicos.TryAddScoped<IEstimadorQualidadeDocumento, EstimadorQualidadeDocumento>();
        servicos.TryAddScoped<IDashboardServico, DashboardServico>();
        servicos.TryAddScoped<IFaturamentoServico, FaturamentoServico>();
        servicos.TryAddScoped<IFormularioAberturaServico, FormularioAberturaServico>();
        servicos.TryAddSingleton<IPublicadorAtualizacaoTempoReal, PublicadorAtualizacaoTempoRealNulo>();
        servicos.TryAddSingleton(TimeProvider.System);

        return servicos;
    }
}
