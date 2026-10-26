using System.Reflection;
using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarAtivoItemCatalogoOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarItemCatalogoOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarItemCatalogoOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ListarCatalogoOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;

namespace Officio.Testes.Unitarios.DominioMediador;

public class OrcamentoHandlersArquiteturaTestes
{
    public static TheoryData<Type> HandlersOrcamento => new()
    {
        typeof(CriarOrcamentoHandler),
        typeof(AtualizarOrcamentoHandler),
        typeof(AceitarOrcamentoHandler),
        typeof(RecusarOrcamentoHandler),
        typeof(AlterarStatusOrcamentoHandler),
        typeof(ObterOrcamentoPorIdHandler),
        typeof(ObterOrcamentosPorProspectoHandler),
        typeof(CriarItemCatalogoOrcamentoHandler),
        typeof(AtualizarItemCatalogoOrcamentoHandler),
        typeof(AlterarAtivoItemCatalogoOrcamentoHandler),
        typeof(ListarCatalogoOrcamentoHandler)
    };

    public static TheoryData<Type> HandlersMediador
    {
        get
        {
            var dados = new TheoryData<Type>();
            var handlers = typeof(CriarOrcamentoHandler).Assembly
                .GetTypes()
                .Where(tipo => !tipo.IsAbstract && tipo.GetInterfaces().Any(EhHandlerMediatr))
                .OrderBy(tipo => tipo.FullName);

            foreach (var handler in handlers)
                dados.Add(handler);

            return dados;
        }
    }

    [Theory]
    [MemberData(nameof(HandlersOrcamento))]
    public void HandlerDeOrcamento_DeveDependerSomenteDoServicoDeOrcamento(Type handlerType)
    {
        var construtor = Assert.Single(handlerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var dependencias = construtor.GetParameters().Select(parametro => parametro.ParameterType).ToArray();

        Assert.Equal([typeof(IOrcamentoServico)], dependencias);
        Assert.DoesNotContain(typeof(IOrcamentoRepositorio), dependencias);
        Assert.DoesNotContain(typeof(IProspectoRepositorio), dependencias);
        Assert.DoesNotContain(typeof(IEmpresaAtual), dependencias);
    }

    [Theory]
    [MemberData(nameof(HandlersMediador))]
    public void HandlersMediador_NaoDevemInjetarRepositoriosOuTenant(Type handlerType)
    {
        var dependencias = handlerType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(construtor => construtor.GetParameters())
            .Select(parametro => parametro.ParameterType)
            .ToArray();

        Assert.DoesNotContain(dependencias, dependencia => dependencia.Name.EndsWith("Repositorio", StringComparison.Ordinal));
        Assert.DoesNotContain(typeof(IEmpresaAtual), dependencias);
    }

    private static bool EhHandlerMediatr(Type tipo)
        => tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(IRequestHandler<,>);
}
