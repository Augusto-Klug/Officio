using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class AdmissaoServicoTestes
{
    [Fact]
    public async Task CriarManual_NormalizaDocumentosEPersisteDependentes()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, cliente) = await CriarClienteAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.CriarManualAsync(new CriarAdmissaoManualRequisicao(
            cliente.Id,
            CriarDados(cpf: "117.892.999-00", dependentes:
            [new("Dependente", "123.456.789-01", null, "Filho", true, false)])));

        Assert.True(resultado.Sucesso);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.Include(item => item.Dependentes).SingleAsync();
        Assert.Equal("11789299900", admissao.Cpf);
        Assert.Equal("12345678901", Assert.Single(admissao.Dependentes).Cpf);
    }

    [Fact]
    public async Task Concluir_CriaFuncionarioEIncrementaQuantidadeUmaUnicaVez()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (empresa, cliente) = await CriarClienteAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);
        var criacao = await servico.CriarManualAsync(new CriarAdmissaoManualRequisicao(
            cliente.Id,
            CriarDados(cpf: "11789299900", dataAdmissao: new DateOnly(2026, 9, 21), funcao: "Analista")));

        var primeiraConclusao = await servico.ConcluirAsync(criacao.Valor);
        var segundaConclusao = await servico.ConcluirAsync(criacao.Valor);

        Assert.True(primeiraConclusao.Sucesso);
        Assert.True(segundaConclusao.Sucesso);
        contexto.ChangeTracker.Clear();
        var funcionario = await contexto.EmpresaFuncionarios.SingleAsync();
        var empresaAtualizada = await contexto.Empresas.SingleAsync(item => item.Id == empresa.Id);
        Assert.Equal(criacao.Valor, funcionario.AdmissaoOrigemId);
        Assert.Equal("Analista", funcionario.Funcao);
        Assert.Equal(1, empresaAtualizada.QuantidadeFuncionariosAtual);
    }

    [Fact]
    public async Task Atualizar_SubstituiDependentesExistentes()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, cliente) = await CriarClienteAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);
        var criacao = await servico.CriarManualAsync(new CriarAdmissaoManualRequisicao(
            cliente.Id,
            CriarDados(dependentes:
            [
                new("Primeiro", null, null, "Filho", true, false),
                new("Segundo", null, null, "Filho", false, false)
            ])));

        var resultado = await servico.AtualizarAsync(new AtualizarAdmissaoRequisicao(
            criacao.Valor,
            CriarDados(dependentes: [new("Novo dependente", null, null, "Conjuge", true, false)])));

        Assert.True(resultado.Sucesso);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.Include(item => item.Dependentes).SingleAsync();
        Assert.Equal("Novo dependente", Assert.Single(admissao.Dependentes).Nome);
        Assert.Single(contexto.AdmissaoDependentes);
    }

    [Fact]
    public async Task CriarManual_ComCpfInvalidoNoDependente_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, cliente) = await CriarClienteAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.CriarManualAsync(new CriarAdmissaoManualRequisicao(
            cliente.Id,
            CriarDados(dependentes: [new("Dependente", "123", null, "Filho", true, false)])));

        Assert.False(resultado.Sucesso);
        Assert.Contains("CPF do dependente", resultado.Erro);
        Assert.Empty(contexto.Admissoes);
    }

    [Fact]
    public async Task Concluir_SemDadosObrigatorios_FalhaSemCriarFuncionario()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, cliente) = await CriarClienteAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);
        var criacao = await servico.CriarManualAsync(new CriarAdmissaoManualRequisicao(
            cliente.Id,
            CriarDados(cpf: null, dataAdmissao: null, funcao: null)));

        var resultado = await servico.ConcluirAsync(criacao.Valor);

        Assert.False(resultado.Sucesso);
        Assert.Contains("CPF", resultado.Erro);
        Assert.Empty(contexto.EmpresaFuncionarios);
    }

    [Fact]
    public async Task Atualizar_QuandoCancelada_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, cliente) = await CriarClienteAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);
        var criacao = await servico.CriarManualAsync(new CriarAdmissaoManualRequisicao(cliente.Id, CriarDados()));
        await servico.CancelarAsync(criacao.Valor);

        var resultado = await servico.AtualizarAsync(new AtualizarAdmissaoRequisicao(criacao.Valor, CriarDados(nome: "Outro nome")));

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao pode ser editada", resultado.Erro);
    }

    [Fact]
    public async Task ObterPorId_NaoRetornaAdmissaoDeOutroTenant()
    {
        var nomeBanco = Guid.NewGuid().ToString();
        var tenantEstrangeiroId = Guid.NewGuid();
        Guid admissaoId;
        await using (var contextoEstrangeiro = CriarContexto(tenantEstrangeiroId, nomeBanco))
        {
            var (_, cliente) = await CriarClienteAsync(contextoEstrangeiro, tenantEstrangeiroId);
            var criacao = await CriarServico(contextoEstrangeiro, tenantEstrangeiroId)
                .CriarManualAsync(new CriarAdmissaoManualRequisicao(cliente.Id, CriarDados()));
            admissaoId = criacao.Valor;
        }

        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId, nomeBanco);
        var resultado = await CriarServico(contexto, tenantId).ObterPorIdAsync(admissaoId);

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrada", resultado.Erro);
    }

    private static DadosAdmissaoRequisicao CriarDados(
        string nome = "Funcionario Teste",
        string? cpf = "11789299900",
        DateOnly? dataAdmissao = null,
        string? funcao = "Assistente",
        IReadOnlyCollection<DependenteAdmissaoRequisicao>? dependentes = null)
        => new(
            nome, cpf, null, null, null, null, null, null, null, null,
            null, null, null, null, dataAdmissao, 2500m, null, funcao,
            90, false, null, null, dependentes ?? []);

    private static async Task<(Empresa Empresa, Cliente Cliente)> CriarClienteAsync(
        ContextoAplicacao contexto,
        Guid tenantId)
    {
        var empresa = Empresa.CriarConstituida("11222333000144", "Empresa Teste");
        var cliente = Cliente.CriarParaEmpresa(tenantId, empresa.Id);
        contexto.Empresas.Add(empresa);
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();
        return (empresa, cliente);
    }

    private static IAdmissaoServico CriarServico(ContextoAplicacao contexto, Guid tenantId)
        => new AdmissaoServico(
            new AdmissaoRepositorio(contexto),
            new ClienteRepositorio(contexto),
            CriarEmpresaAtual(tenantId));

    private static ContextoAplicacao CriarContexto(Guid tenantId, string? nomeBanco = null)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(nomeBanco ?? Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, CriarEmpresaAtual(tenantId), new PublicadorEventosFake());
    }

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return empresaAtual;
    }
}
