using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AtualizarAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CancelarAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CriarAdmissaoManual;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarAdmissoes;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterAdmissaoPorId;

namespace Officio.Testes.Unitarios.DominioMediador;

public class AdmissaoHandlersArquiteturaTestes
{
    public static TheoryData<Type> Handlers => new()
    {
        typeof(CriarAdmissaoManualHandler),
        typeof(AtualizarAdmissaoHandler),
        typeof(ConcluirAdmissaoHandler),
        typeof(CancelarAdmissaoHandler),
        typeof(ListarAdmissoesHandler),
        typeof(ObterAdmissaoPorIdHandler)
    };

    [Theory]
    [MemberData(nameof(Handlers))]
    public void Handler_DeveDependerSomenteDoServicoDeAdmissao(Type handlerType)
    {
        var construtor = Assert.Single(handlerType.GetConstructors());
        var dependencia = Assert.Single(construtor.GetParameters());
        Assert.Equal(typeof(IAdmissaoServico), dependencia.ParameterType);
    }
}
