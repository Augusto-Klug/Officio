using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class FichaAdmissaoServicoTestes
{
    [Fact]
    public async Task GerarLink_ArmazenaSomenteHashEInvalidaTokenAnterior()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;

        var primeiro = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        var segundo = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);

        Assert.True(primeiro.Sucesso);
        Assert.True(segundo.Sucesso);
        Assert.NotEqual(primeiro.Valor!.Token, segundo.Valor!.Token);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.IgnoreQueryFilters().SingleAsync();
        Assert.NotEqual(segundo.Valor.Token, admissao.TokenAcessoHash);
        Assert.Equal(64, admissao.TokenAcessoHash!.Length);
        Assert.False((await cenario.Servico.ObterAsync(primeiro.Valor.Token)).Sucesso);
        Assert.True((await cenario.Servico.ObterAsync(segundo.Valor.Token)).Sucesso);
    }

    [Fact]
    public async Task AtualizarFicha_PreservaDadosDefinidosPelaEmpresa()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var link = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        var requisicao = new AtualizarFichaAdmissaoRequisicao(
            "Funcionario Atualizado", "11789299900", "123", new DateOnly(1995, 5, 10),
            "Feminino", "Solteira", "Superior", "47999999999", "ana@example.com", null,
            "Banco", "1", "2", "Corrente", true, []);

        var resultado = await cenario.Servico.AtualizarAsync(link.Valor!.Token, requisicao);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("Funcionario Atualizado", admissao.NomeFuncionario);
        Assert.Equal("Analista", admissao.Funcao);
        Assert.Equal(3500m, admissao.Salario);
        Assert.Equal(new DateOnly(2026, 10, 1), admissao.DataAdmissaoPrevista);
    }

    [Fact]
    public async Task ConcluirFicha_SemDocumentosObrigatorios_Rejeita()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var link = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);

        var resultado = await cenario.Servico.ConcluirAsync(link.Valor!.Token);

        Assert.False(resultado.Sucesso);
        Assert.Contains("documentos obrigatorios", resultado.Erro);
    }

    [Fact]
    public async Task ObterFicha_ComTokenValido_ResolveTenantSemAutenticacao()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var link = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        cenario.EmpresaAtual.SetTenant(Guid.Empty);

        var resultado = await cenario.Servico.ObterAsync(link.Valor!.Token);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(cenario.TenantId, cenario.EmpresaAtual.TenantId);
        Assert.Equal("Empresa Teste", resultado.Valor!.EmpresaNome);
        Assert.Equal(7, resultado.Valor.Documentos.Count);
    }

    [Fact]
    public async Task RevogarLink_InvalidaTokenERetornaAdmissaoParaRascunho()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var link = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);

        var resultado = await cenario.Servico.RevogarLinkAsync(cenario.AdmissaoId);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.False((await cenario.Servico.ObterAsync(link.Valor!.Token)).Sucesso);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .SingleAsync();
        Assert.Null(admissao.TokenAcessoHash);
        Assert.Equal(StatusAdmissaoCodigos.Rascunho, admissao.StatusAdmissao.Codigo);
    }

    [Fact]
    public async Task ConcluirFicha_ComDadosEDocumentosObrigatorios_EnviaParaValidacao()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var link = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        await PreencherDocumentosObrigatoriosAsync(contexto, cenario);

        var resultado = await cenario.Servico.ConcluirAsync(link.Valor!.Token);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .SingleAsync();
        Assert.NotNull(admissao.PreenchimentoFuncionarioConcluidoEm);
        Assert.Equal(StatusAdmissaoCodigos.EmValidacao, admissao.StatusAdmissao.Codigo);
        Assert.Equal(1, cenario.DocumentoServico.ValidacoesEmLote);
        Assert.False((await cenario.Servico.AtualizarAsync(link.Valor.Token,
            new("Outro nome", null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, []))).Sucesso);
    }

    private static async Task<Cenario> CriarCenarioAsync()
    {
        var tenantId = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var contexto = new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
        var empresa = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa Teste");
        var cliente = Cliente.CriarParaEmpresa(tenantId, empresa.Id);
        contexto.Empresas.Add(empresa);
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();

        var repositorio = new AdmissaoRepositorio(contexto);
        var admissaoServico = new AdmissaoServico(repositorio, new ClienteRepositorio(contexto), empresaAtual);
        var dados = new DadosAdmissaoRequisicao(
            "Funcionario Teste", "11789299900", null, new DateOnly(1995, 5, 10), null, null,
            null, "47999999999", null, null, null, null, null, null, new DateOnly(2026, 10, 1),
            3500m, null, "Analista", 90, false, null, null, []);
        var criacao = await admissaoServico.CriarManualAsync(new(cliente.Id, dados));
        var documentoServico = new DocumentoAdmissaoServicoFake();
        var servico = new FichaAdmissaoServico(repositorio, documentoServico, empresaAtual);
        return new(contexto, servico, documentoServico, empresaAtual, tenantId, criacao.Valor);
    }

    private static async Task PreencherDocumentosObrigatoriosAsync(ContextoAplicacao contexto, Cenario cenario)
    {
        var admissao = await contexto.Admissoes
            .Include(item => item.Documentos)
                .ThenInclude(item => item.TipoDocumento)
            .SingleAsync(item => item.Id == cenario.AdmissaoId);
        foreach (var item in admissao.Documentos.Where(item =>
                     item.Obrigatorio || item.TipoDocumento.Codigo == "IDENTIDADE"))
        {
            var documento = Documento.CriarAdmissao(
                cenario.TenantId, admissao.Id, admissao.EmpresaId, item.TipoDocumentoId,
                $"{item.Id}.pdf", Officio.Dominio.Enumeradores.TipoDocumento.Identidade,
                $"admissoes/{admissao.Id}/{item.Id}.pdf", "application/pdf", 100, 1m);
            item.VincularDocumento(documento);
            contexto.Documents.Add(documento);
        }
        await contexto.SaveChangesAsync();
    }

    private sealed record Cenario(
        ContextoAplicacao Contexto,
        FichaAdmissaoServico Servico,
        DocumentoAdmissaoServicoFake DocumentoServico,
        ServicoEmpresaAtual EmpresaAtual,
        Guid TenantId,
        Guid AdmissaoId);

    private sealed class DocumentoAdmissaoServicoFake : IAdmissaoDocumentoServico
    {
        public int ValidacoesEmLote { get; private set; }

        public Task<Resultado<List<AdmissaoDocumentoDto>>> ListarAsync(Guid admissaoId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarAsync(EnviarDocumentoAdmissaoRequisicao requisicao, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<ValidationResultDto>> ValidarAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<ResumoValidacaoDocumentosAdmissaoDto>> ValidarPendentesAsync(Guid admissaoId, CancellationToken ct = default)
        {
            ValidacoesEmLote++;
            return Task.FromResult(Resultado<ResumoValidacaoDocumentosAdmissaoDto>.CriarSucesso(new(0, 0, 0, 0)));
        }
        public Task<Resultado<bool>> AprovarManualmenteAsync(AprovarDocumentoAdmissaoRequisicao requisicao, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<bool>> RemoverAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
