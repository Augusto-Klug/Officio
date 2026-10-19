using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
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
        Assert.Equal(2, cenario.Publicador.Eventos.Count);
        Assert.All(cenario.Publicador.Eventos, evento => Assert.Equal(
            new EventoAtualizacaoTempoReal(
                cenario.TenantId, EntidadesAtualizacaoTempoReal.Admissao, cenario.AdmissaoId),
            evento));
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.IgnoreQueryFilters().SingleAsync();
        Assert.NotEqual(segundo.Valor.Token, admissao.TokenAcessoHash);
        Assert.Equal(64, admissao.TokenAcessoHash!.Length);
        Assert.False((await cenario.Servico.ObterAsync(primeiro.Valor.Token)).Sucesso);
        Assert.True((await cenario.Servico.ObterAsync(segundo.Valor.Token)).Sucesso);
    }

    [Fact]
    public async Task EnviarLinkPorEmail_EnviaLinkAtivoDoAdmitido()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var link = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);

        var resultado = await cenario.Servico.EnviarLinkPorEmailAsync(new(
            cenario.AdmissaoId, "funcionario", "admitido@example.com", link.Valor!.Token));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal("admitido@example.com", cenario.Email.UltimoDestinatario);
        Assert.Contains($"https://officio.test/ficha-admissao/{link.Valor.Token}", cenario.Email.UltimoCorpo);
        Assert.Single(cenario.Publicador.Eventos);
    }

    [Fact]
    public async Task AtualizarFicha_PreservaDadosDefinidosPelaEmpresa()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var link = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        var requisicao = new AtualizarFichaFuncionarioRequisicao(
            "Ana Atualizada", new DateOnly(1994, 3, 2), "Feminino",
            "Solteira(a)", "Ensino superior completo", "47999999999", "ana@example.com", []);

        var resultado = await cenario.Servico.AtualizarAsync(link.Valor!.Token, requisicao);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("Ana Atualizada", admissao.NomeFuncionario);
        Assert.Equal(new DateOnly(1994, 3, 2), admissao.DataNascimento);
        Assert.Equal("Analista", admissao.Funcao);
        Assert.Equal(3500m, admissao.Salario);
        Assert.Equal(new DateOnly(2026, 10, 1), admissao.DataAdmissaoPrevista);
        Assert.Equal(2, cenario.Publicador.Eventos.Count);
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
        Assert.Single(cenario.Publicador.Eventos);
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
        Assert.Equal(2, cenario.Publicador.Eventos.Count);
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
        Assert.Equal(StatusAdmissaoCodigos.AguardandoValidacao, admissao.StatusAdmissao.Codigo);
        Assert.Equal(0, cenario.DocumentoServico.Validacoes);
        Assert.Equal(2, cenario.Publicador.Eventos.Count);
        Assert.False((await cenario.Servico.AtualizarAsync(link.Valor.Token,
            new("Funcionario Teste", new DateOnly(1995, 5, 10), "Feminino", "Solteiro(a)", null, null, null, []))).Sucesso);
    }

    [Fact]
    public async Task GerarLinkEmpresa_SoLiberaDepoisDaConclusaoDoAdmitido_EMantemTokensSeparados()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var linkFuncionario = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);

        var antesDaConclusao = await cenario.Servico.GerarLinkEmpresaAsync(cenario.AdmissaoId);
        Assert.False(antesDaConclusao.Sucesso);

        await PreencherDocumentosObrigatoriosAsync(contexto, cenario);
        var conclusaoFuncionario = await cenario.Servico.ConcluirAsync(linkFuncionario.Valor!.Token);
        Assert.True(conclusaoFuncionario.Sucesso, conclusaoFuncionario.Erro);

        var linkAntesDaRevisao = await cenario.Servico.GerarLinkEmpresaAsync(cenario.AdmissaoId);
        Assert.False(linkAntesDaRevisao.Sucesso);
        Assert.Contains("revisao documental", linkAntesDaRevisao.Erro);

        var revisao = await cenario.AdmissaoServico.ConcluirRevisaoDocumentalAsync(cenario.AdmissaoId);
        Assert.True(revisao.Sucesso, revisao.Erro);
        Assert.Equal(3, cenario.Publicador.Eventos.Count);
        var linkEmpresa = await cenario.Servico.GerarLinkEmpresaAsync(cenario.AdmissaoId);
        Assert.True(linkEmpresa.Sucesso, linkEmpresa.Erro);
        Assert.NotEqual(linkFuncionario.Valor.Token, linkEmpresa.Valor!.Token);
        Assert.False((await cenario.Servico.ObterEmpresaAsync(linkFuncionario.Valor.Token)).Sucesso);
        Assert.False((await cenario.Servico.ObterAsync(linkEmpresa.Valor.Token)).Sucesso);

        var limparCamposObrigatorios = await cenario.Servico.AtualizarEmpresaAsync(linkEmpresa.Valor.Token,
            new(null, null, null, null, null, null, null, null, null, null, null, null));
        Assert.True(limparCamposObrigatorios.Sucesso, limparCamposObrigatorios.Erro);
        var semDataAdmissao = await cenario.Servico.ConcluirEmpresaAsync(linkEmpresa.Valor.Token);
        Assert.False(semDataAdmissao.Sucesso);
        Assert.Contains("Data de admissao", semDataAdmissao.Erro);

        var somenteDataAdmissao = await cenario.Servico.AtualizarEmpresaAsync(linkEmpresa.Valor.Token,
            new(null, new DateOnly(2026, 11, 2), null, null, null, null, null, null, null, null, null, null));
        Assert.True(somenteDataAdmissao.Sucesso, somenteDataAdmissao.Erro);
        var semFuncao = await cenario.Servico.ConcluirEmpresaAsync(linkEmpresa.Valor.Token);
        Assert.False(semFuncao.Sucesso);
        Assert.Contains("Funcao", semFuncao.Erro);

        var atualizacao = await cenario.Servico.AtualizarEmpresaAsync(linkEmpresa.Valor.Token,
            new("12345678901", new DateOnly(2026, 11, 2), 4200m, "5%", "Analista Senior", 90, true,
                "08:00-18:00", "Banco Teste", "0001", "12345-6", "Corrente"));
        Assert.True(atualizacao.Sucesso, atualizacao.Erro);
        var conclusaoEmpresa = await cenario.Servico.ConcluirEmpresaAsync(linkEmpresa.Valor.Token);
        Assert.True(conclusaoEmpresa.Sucesso, conclusaoEmpresa.Erro);
        Assert.False((await cenario.Servico.AtualizarEmpresaAsync(linkEmpresa.Valor.Token,
            new(null, null, null, null, null, null, null, null, null, null, null, null))).Sucesso);

        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.IgnoreQueryFilters().SingleAsync();
        Assert.Equal("Funcionario Teste", admissao.NomeFuncionario);
        Assert.Equal("11789299900", admissao.Cpf);
        Assert.Equal("12345678901", admissao.Pis);
        Assert.Equal("Analista Senior", admissao.Funcao);
        Assert.Equal("Banco Teste", admissao.Banco);
        Assert.Equal("0001", admissao.Agencia);
        Assert.Equal("12345-6", admissao.Conta);
        Assert.Equal("Corrente", admissao.TipoConta);
        Assert.NotNull(admissao.PreenchimentoEmpresaConcluidoEm);
        Assert.Equal(8, cenario.Publicador.Eventos.Count);
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
        var publicador = new PublicadorAtualizacaoFake();
        var admissaoServico = new AdmissaoServico(repositorio, new ClienteRepositorio(contexto), empresaAtual, publicador);
        var dados = new DadosAdmissaoRequisicao(
            "Funcionario Teste", "11789299900", "1234567", new DateOnly(1995, 5, 10), "Feminino", null,
            null, "47999999999", null, null, null, null, null, null, new DateOnly(2026, 10, 1),
            3500m, null, "Analista", 90, false, null, null, []);
        var criacao = await admissaoServico.CriarManualAsync(new(cliente.Id, dados));
        var documentoServico = new DocumentoAdmissaoServicoFake();
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Acesso:FrontendBaseUrl"] = "https://officio.test"
            })
            .Build();
        var email = new EmailFake();
        var servico = new FichaAdmissaoServico(repositorio, documentoServico, empresaAtual, email, configuracao, publicador);
        publicador.Eventos.Clear();
        return new(contexto, servico, documentoServico, admissaoServico, empresaAtual, email, publicador, tenantId, criacao.Valor);
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
            contexto.DocumentValidations.Add(ValidacaoDocumento.Create(
                cenario.TenantId, documento.Id, [], [], true, 1m, item.Id));
        }
        await contexto.SaveChangesAsync();
    }

    private sealed record Cenario(
        ContextoAplicacao Contexto,
        FichaAdmissaoServico Servico,
        DocumentoAdmissaoServicoFake DocumentoServico,
        AdmissaoServico AdmissaoServico,
        ServicoEmpresaAtual EmpresaAtual,
        EmailFake Email,
        PublicadorAtualizacaoFake Publicador,
        Guid TenantId,
        Guid AdmissaoId);

    private sealed class PublicadorAtualizacaoFake : IPublicadorAtualizacaoTempoReal
    {
        public List<EventoAtualizacaoTempoReal> Eventos { get; } = [];

        public Task PublicarAsync(EventoAtualizacaoTempoReal evento, CancellationToken cancellationToken = default)
        {
            Eventos.Add(evento);
            return Task.CompletedTask;
        }
    }

    private sealed class DocumentoAdmissaoServicoFake : IAdmissaoDocumentoServico
    {
        public int Validacoes { get; private set; }

        public Task<Resultado<List<AdmissaoDocumentoDto>>> ListarAsync(Guid admissaoId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarAsync(EnviarDocumentoAdmissaoRequisicao requisicao, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<ValidationResultDto>> ValidarAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default)
        {
            Validacoes++;
            return Task.FromResult(Resultado<ValidationResultDto>.CriarFalha("Nao deveria iniciar OCR na submissao publica."));
        }
        public Task<Resultado<bool>> AprovarManualmenteAsync(AprovarDocumentoAdmissaoRequisicao requisicao, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Resultado<bool>> RemoverAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class EmailFake : IServicoEmail
    {
        public string? UltimoDestinatario { get; private set; }
        public string? UltimoCorpo { get; private set; }

        public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            UltimoDestinatario = to;
            UltimoCorpo = body;
            return Task.CompletedTask;
        }
    }
}
