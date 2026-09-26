using System.Reflection;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarEmpresasContratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;

namespace Officio.Testes.Unitarios.DominioMediador;

public class ContratoHandlersArquiteturaTestes
{
    public static TheoryData<Type> HandlersContrato => new()
    {
        typeof(ConverterOrcamentoParaContratoHandler),
        typeof(ListarContratosHandler),
        typeof(ListarEmpresasContratosHandler),
        typeof(ObterContratoPorIdHandler)
    };

    [Theory]
    [MemberData(nameof(HandlersContrato))]
    public void HandlerDeContrato_DeveDependerSomenteDoServicoDeContrato(Type handlerType)
    {
        var construtor = Assert.Single(handlerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var dependencias = construtor.GetParameters().Select(parametro => parametro.ParameterType).ToArray();

        Assert.Equal([typeof(IContratoServico)], dependencias);
    }
}
