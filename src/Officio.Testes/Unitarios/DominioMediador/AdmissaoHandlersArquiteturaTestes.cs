using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AtualizarAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CancelarAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CriarAdmissaoManual;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AprovarDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.EnviarDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RemoverDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ValidarDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarAdmissoes;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarDocumentosAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterAdmissaoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterUrlDocumentoAdmissao;

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

    public static TheoryData<Type> HandlersDocumentos => new()
    {
        typeof(EnviarDocumentoAdmissaoHandler),
        typeof(ValidarDocumentoAdmissaoHandler),
        typeof(AprovarDocumentoAdmissaoHandler),
        typeof(RemoverDocumentoAdmissaoHandler),
        typeof(ListarDocumentosAdmissaoHandler),
        typeof(ObterUrlDocumentoAdmissaoHandler)
    };

    [Theory]
    [MemberData(nameof(Handlers))]
    public void Handler_DeveDependerSomenteDoServicoDeAdmissao(Type handlerType)
    {
        var construtor = Assert.Single(handlerType.GetConstructors());
        var dependencia = Assert.Single(construtor.GetParameters());
        Assert.Equal(typeof(IAdmissaoServico), dependencia.ParameterType);
    }

    [Theory]
    [MemberData(nameof(HandlersDocumentos))]
    public void HandlerDocumento_DeveDependerSomenteDoServicoDeDocumentoDaAdmissao(Type handlerType)
    {
        var construtor = Assert.Single(handlerType.GetConstructors());
        var dependencia = Assert.Single(construtor.GetParameters());
        Assert.Equal(typeof(IAdmissaoDocumentoServico), dependencia.ParameterType);
    }
}
