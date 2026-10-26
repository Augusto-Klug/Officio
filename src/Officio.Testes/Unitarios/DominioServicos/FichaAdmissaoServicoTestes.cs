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
    public async Task RevogarLink_InvalidaTokenSemRetrocederEtapaDaAdmissao()
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
        Assert.Equal(StatusAdmissaoCodigos.AguardandoAdmitido, admissao.StatusAdmissao.Codigo);
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
    public async Task FichasIndependentes_EmpregadorConcluiPrimeiro_AguardaValidacaoFinal()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var linkFuncionario = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        var linkEmpresa = await cenario.Servico.GerarLinkEmpresaAsync(cenario.AdmissaoId);

        Assert.True(linkFuncionario.Sucesso, linkFuncionario.Erro);
        Assert.True(linkEmpresa.Sucesso, linkEmpresa.Erro);
        Assert.True((await cenario.Servico.ObterEmpresaAsync(linkEmpresa.Valor!.Token)).Sucesso);

        var dadosEmpresa = new AtualizarFichaEmpresaRequisicao(
            "12345678901", new DateOnly(2026, 11, 2), 4200m, "5%", "Analista Senior", 90,
            true, "08:00-18:00", "Banco Teste", "0001", "12345-6", "Corrente");
        Assert.True((await cenario.Servico.AtualizarEmpresaAsync(linkEmpresa.Valor.Token, dadosEmpresa)).Sucesso);
        Assert.True((await cenario.Servico.ConcluirEmpresaAsync(linkEmpresa.Valor.Token)).Sucesso);

        var novoLinkFuncionario = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        Assert.True(novoLinkFuncionario.Sucesso, novoLinkFuncionario.Erro);
        Assert.False((await cenario.Servico.ObterAsync(linkFuncionario.Valor!.Token)).Sucesso);
        Assert.True((await cenario.Servico.ObterEmpresaAsync(linkEmpresa.Valor.Token)).Sucesso);

        contexto.ChangeTracker.Clear();
        var aposEnvioEmpresa = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .SingleAsync();
        Assert.NotNull(aposEnvioEmpresa.PreenchimentoEmpresaConcluidoEm);
        Assert.Equal(StatusAdmissaoCodigos.AguardandoAdmitido, aposEnvioEmpresa.StatusAdmissao.Codigo);

        await PreencherDocumentosObrigatoriosAsync(contexto, cenario);
        var conclusaoFuncionario = await cenario.Servico.ConcluirAsync(novoLinkFuncionario.Valor!.Token);
        Assert.True(conclusaoFuncionario.Sucesso, conclusaoFuncionario.Erro);
        Assert.False((await cenario.Servico.AtualizarAsync(novoLinkFuncionario.Valor.Token,
            new("Funcionario Teste", new DateOnly(1995, 5, 10), "Feminino", null, null, null, null, []))).Sucesso);

        var revisao = await cenario.AdmissaoServico.ConcluirRevisaoDocumentalAsync(cenario.AdmissaoId);
        Assert.True(revisao.Sucesso, revisao.Erro);
        contexto.ChangeTracker.Clear();
        var liberada = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .SingleAsync();
        Assert.Equal(StatusAdmissaoCodigos.LiberadoParaRevisaoFinal, liberada.StatusAdmissao.Codigo);

        var concluida = await cenario.AdmissaoServico.ConcluirAsync(cenario.AdmissaoId);
        Assert.True(concluida.Sucesso, concluida.Erro);
    }

    [Fact]
    public async Task FichasIndependentes_FuncionarioConcluiPrimeiro_ValidacaoPodeAntecederEmpregador()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var linkFuncionario = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        var linkEmpresa = await cenario.Servico.GerarLinkEmpresaAsync(cenario.AdmissaoId);
        Assert.True(linkFuncionario.Sucesso, linkFuncionario.Erro);
        Assert.True(linkEmpresa.Sucesso, linkEmpresa.Erro);

        await PreencherDocumentosObrigatoriosAsync(contexto, cenario);
        var conclusaoFuncionario = await cenario.Servico.ConcluirAsync(linkFuncionario.Valor!.Token);
        Assert.True(conclusaoFuncionario.Sucesso, conclusaoFuncionario.Erro);
        var validacaoAntesDoEmpregador = await cenario.AdmissaoServico.ConcluirRevisaoDocumentalAsync(cenario.AdmissaoId);
        Assert.True(validacaoAntesDoEmpregador.Sucesso, validacaoAntesDoEmpregador.Erro);

        contexto.ChangeTracker.Clear();
        var aguardandoEmpresa = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .SingleAsync();
        Assert.Equal(StatusAdmissaoCodigos.AguardandoEmpregador, aguardandoEmpresa.StatusAdmissao.Codigo);
        Assert.True((await cenario.Servico.ObterEmpresaAsync(linkEmpresa.Valor!.Token)).Sucesso);

        var dadosEmpresa = new AtualizarFichaEmpresaRequisicao(
            "12345678901", new DateOnly(2026, 11, 2), 4200m, "5%", "Analista Senior", 90,
            true, "08:00-18:00", "Banco Teste", "0001", "12345-6", "Corrente");
        Assert.True((await cenario.Servico.AtualizarEmpresaAsync(linkEmpresa.Valor.Token, dadosEmpresa)).Sucesso);
        Assert.True((await cenario.Servico.ConcluirEmpresaAsync(linkEmpresa.Valor.Token)).Sucesso);

        contexto.ChangeTracker.Clear();
        var liberada = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .SingleAsync();
        Assert.Equal(StatusAdmissaoCodigos.LiberadoParaRevisaoFinal, liberada.StatusAdmissao.Codigo);
        Assert.Equal("Analista Senior", liberada.Funcao);
        Assert.Equal("12345678901", liberada.Pis);
        Assert.NotNull(liberada.PreenchimentoFuncionarioConcluidoEm);
        Assert.NotNull(liberada.PreenchimentoEmpresaConcluidoEm);
    }

    [Fact]
    public async Task CorrecaoDeIdentificacaoPelaContabilidade_MantemFichasFechadasEMarcaDocumentosParaRevisao()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var linkFuncionario = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        var linkEmpresa = await cenario.Servico.GerarLinkEmpresaAsync(cenario.AdmissaoId);
        await PreencherDocumentosObrigatoriosAsync(contexto, cenario);
        Assert.True((await cenario.Servico.ConcluirAsync(linkFuncionario.Valor!.Token)).Sucesso);
        Assert.True((await cenario.Servico.ConcluirEmpresaAsync(linkEmpresa.Valor!.Token)).Sucesso);
        Assert.True((await cenario.AdmissaoServico.ConcluirRevisaoDocumentalAsync(cenario.AdmissaoId)).Sucesso);

        var admissao = await contexto.Admissoes
            .Include(item => item.Documentos)
            .SingleAsync(item => item.Id == cenario.AdmissaoId);
        var preenchimentoFuncionario = admissao.PreenchimentoFuncionarioConcluidoEm;
        var preenchimentoEmpresa = admissao.PreenchimentoEmpresaConcluidoEm;
        var dadosCorrigidos = new DadosAdmissaoRequisicao(
            "Funcionario Corrigido", admissao.Cpf, admissao.Rg, admissao.DataNascimento, admissao.Sexo,
            admissao.EstadoCivil, admissao.Escolaridade, admissao.Telefone, admissao.Email, admissao.Pis,
            admissao.Banco, admissao.Agencia, admissao.Conta, admissao.TipoConta,
            admissao.DataAdmissaoPrevista, admissao.Salario, admissao.Comissao, admissao.Funcao,
            admissao.ContratoExperienciaDias, admissao.UtilizaValeTransporte, admissao.HorarioTrabalho,
            admissao.Observacoes, []);

        var atualizacao = await cenario.AdmissaoServico.AtualizarAsync(
            new AtualizarAdmissaoRequisicao(cenario.AdmissaoId, dadosCorrigidos));

        Assert.True(atualizacao.Sucesso, atualizacao.Erro);
        contexto.ChangeTracker.Clear();
        var atualizado = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .Include(item => item.Documentos)
            .SingleAsync();
        Assert.Equal(StatusAdmissaoCodigos.AguardandoValidacao, atualizado.StatusAdmissao.Codigo);
        Assert.Equal(preenchimentoFuncionario, atualizado.PreenchimentoFuncionarioConcluidoEm);
        Assert.Equal(preenchimentoEmpresa, atualizado.PreenchimentoEmpresaConcluidoEm);
        Assert.All(atualizado.Documentos.Where(item => item.DocumentoId.HasValue), item => Assert.True(item.RevisaoPendente));
        var revisaoPendente = await cenario.AdmissaoServico.ConcluirRevisaoDocumentalAsync(cenario.AdmissaoId);
        Assert.False(revisaoPendente.Sucesso);
        Assert.Contains("revisado novamente", revisaoPendente.Erro);
        Assert.True((await cenario.Servico.ObterAsync(linkFuncionario.Valor.Token)).Sucesso);
        Assert.False((await cenario.Servico.AtualizarAsync(linkFuncionario.Valor.Token,
            new("Funcionario Corrigido", new DateOnly(1995, 5, 10), "Feminino", null, null, null, null, []))).Sucesso);
        Assert.True((await cenario.Servico.ObterEmpresaAsync(linkEmpresa.Valor.Token)).Sucesso);
        Assert.False((await cenario.Servico.AtualizarEmpresaAsync(linkEmpresa.Valor.Token,
            new("12345678901", new DateOnly(2026, 11, 2), 4200m, null, "Analista", 90,
                false, null, null, null, null, null))).Sucesso);
        Assert.False((await cenario.AdmissaoServico.ConcluirAsync(cenario.AdmissaoId)).Sucesso);
    }

    [Fact]
    public async Task EdicaoDeDadosTrabalhistasAposEnvios_NaoReabreRevisaoDocumental()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var linkFuncionario = await cenario.Servico.GerarLinkAsync(cenario.AdmissaoId);
        var linkEmpresa = await cenario.Servico.GerarLinkEmpresaAsync(cenario.AdmissaoId);
        await PreencherDocumentosObrigatoriosAsync(contexto, cenario);
        Assert.True((await cenario.Servico.ConcluirAsync(linkFuncionario.Valor!.Token)).Sucesso);
        Assert.True((await cenario.Servico.ConcluirEmpresaAsync(linkEmpresa.Valor!.Token)).Sucesso);
        Assert.True((await cenario.AdmissaoServico.ConcluirRevisaoDocumentalAsync(cenario.AdmissaoId)).Sucesso);

        var admissao = await contexto.Admissoes.SingleAsync(item => item.Id == cenario.AdmissaoId);
        var dadosAtualizados = new DadosAdmissaoRequisicao(
            admissao.NomeFuncionario, admissao.Cpf, admissao.Rg, admissao.DataNascimento, admissao.Sexo,
            admissao.EstadoCivil, admissao.Escolaridade, admissao.Telefone, admissao.Email, "98765432100",
            admissao.Banco, admissao.Agencia, admissao.Conta, admissao.TipoConta,
            admissao.DataAdmissaoPrevista, admissao.Salario, admissao.Comissao, "Analista Senior",
            admissao.ContratoExperienciaDias, admissao.UtilizaValeTransporte, admissao.HorarioTrabalho,
            admissao.Observacoes, []);

        var atualizacao = await cenario.AdmissaoServico.AtualizarAsync(
            new AtualizarAdmissaoRequisicao(cenario.AdmissaoId, dadosAtualizados));

        Assert.True(atualizacao.Sucesso, atualizacao.Erro);
        contexto.ChangeTracker.Clear();
        var atualizado = await contexto.Admissoes
            .IgnoreQueryFilters()
            .Include(item => item.StatusAdmissao)
            .Include(item => item.Documentos)
            .SingleAsync();
        Assert.Equal(StatusAdmissaoCodigos.LiberadoParaRevisaoFinal, atualizado.StatusAdmissao.Codigo);
        Assert.All(atualizado.Documentos, item => Assert.False(item.RevisaoPendente));
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
