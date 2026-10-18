using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class AdmissaoDocumentoServicoTestes
{
    [Fact]
    public async Task Enviar_SubstituirERemover_MantemUmUnicoDocumentoAtivo()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();

        var primeiro = await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade-1.pdf"));
        var segundo = await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade-2.pdf"));

        Assert.True(primeiro.Sucesso);
        Assert.True(segundo.Sucesso);
        Assert.Single(await contexto.Documents.ToListAsync());
        Assert.Contains(cenario.Armazenamento.Excluidos, caminho => caminho.Contains("identidade-1.pdf"));

        var listagem = await cenario.Servico.ListarAsync(cenario.AdmissaoId);
        var documento = Assert.Single(listagem.Valor!, documento => documento.Id == item.Id);
        Assert.Equal("identidade-2.pdf", documento.NomeArquivo);
        Assert.Equal("ENVIADO", documento.StatusValidacao);

        var url = await cenario.Servico.ObterUrlAsync(cenario.AdmissaoId, item.Id);
        Assert.True(url.Sucesso);
        Assert.Contains("identidade-2.pdf", url.Valor!.Url);

        var remocao = await cenario.Servico.RemoverAsync(cenario.AdmissaoId, item.Id);
        Assert.True(remocao.Sucesso);
        Assert.Empty(await contexto.Documents.ToListAsync());
        Assert.Null((await contexto.AdmissaoDocumentos.SingleAsync(documento => documento.Id == item.Id)).DocumentoId);
    }

    [Fact]
    public async Task AlterarDocumento_AposRevisaoDocumentalConcluida_Falha()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        var envioInicial = await cenario.Servico.EnviarAsync(
            CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade.pdf"));
        Assert.True(envioInicial.Sucesso);

        var repositorio = new AdmissaoRepositorio(contexto);
        var admissao = await repositorio.ObterParaAlteracaoAsync(cenario.AdmissaoId);
        var status = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.DocumentosValidados, "Documentos validados", 4);
        admissao!.AlterarStatus(status.Id);
        await contexto.SaveChangesAsync();

        var substituicao = await cenario.Servico.EnviarAsync(
            CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade-atualizada.pdf"));
        var remocao = await cenario.Servico.RemoverAsync(cenario.AdmissaoId, item.Id);

        Assert.False(substituicao.Sucesso);
        Assert.False(remocao.Sucesso);
        Assert.Contains("revisao documental", substituicao.Erro);
        Assert.Contains("revisao documental", remocao.Erro);
    }

    [Fact]
    public async Task Validar_QuandoRequerRevisao_MantemEtapaAguardandoContabilidade()
    {
        var resultadoValidacao = new ValidationResultDto(false, 0.2m, [], true, "Revisao necessaria");
        var cenario = await CriarCenarioAsync(Resultado<ValidationResultDto>.CriarSucesso(resultadoValidacao));
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade.pdf"));

        var resultado = await cenario.Servico.ValidarAsync(cenario.AdmissaoId, item.Id);

        Assert.True(resultado.Sucesso);
        contexto.ChangeTracker.Clear();
        var admissao = await contexto.Admissoes.Include(registro => registro.StatusAdmissao).SingleAsync();
        Assert.Equal(StatusAdmissaoCodigos.AguardandoValidacao, admissao.StatusAdmissao.Codigo);
    }

    [Fact]
    public async Task AprovarManualmente_AprovaUltimaValidacao()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade.pdf"));
        var documentoId = (await contexto.AdmissaoDocumentos.SingleAsync(registro => registro.Id == item.Id)).DocumentoId!.Value;
        contexto.DocumentValidations.Add(ValidacaoDocumento.RequireManualReview(
            cenario.TenantId, documentoId, observacao: "OCR indisponivel", admissaoDocumentoId: item.Id));
        await contexto.SaveChangesAsync();

        var resultado = await cenario.Servico.AprovarManualmenteAsync(
            new AprovarDocumentoAdmissaoRequisicao(cenario.AdmissaoId, item.Id, "Conferido manualmente"));

        Assert.True(resultado.Sucesso);
        contexto.ChangeTracker.Clear();
        var validacao = await contexto.DocumentValidations.SingleAsync();
        Assert.Equal(StatusValidacao.Aprovado, validacao.Status);
        Assert.Equal("Conferido manualmente", validacao.ObservacaoRevisao);
        Assert.Equal(cenario.RevisorId, validacao.RevisadoPorId);
        Assert.NotNull(validacao.RevisadoEm);
    }

    [Fact]
    public async Task AprovarManualmente_ExigeJustificativa()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade.pdf"));
        var documentoId = (await contexto.AdmissaoDocumentos.SingleAsync(registro => registro.Id == item.Id)).DocumentoId!.Value;
        contexto.DocumentValidations.Add(ValidacaoDocumento.RequireManualReview(
            cenario.TenantId, documentoId, observacao: "Baixa confianca", admissaoDocumentoId: item.Id));
        await contexto.SaveChangesAsync();

        var resultado = await cenario.Servico.AprovarManualmenteAsync(
            new AprovarDocumentoAdmissaoRequisicao(cenario.AdmissaoId, item.Id, "  "));

        Assert.False(resultado.Sucesso);
        Assert.Contains("justificativa", resultado.Erro);
    }

    [Fact]
    public async Task AprovarManualmente_NaoPermiteReaprovarDocumentoJaAprovado()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, item.Id, "identidade.pdf"));
        var documentoId = (await contexto.AdmissaoDocumentos.SingleAsync(registro => registro.Id == item.Id)).DocumentoId!.Value;
        contexto.DocumentValidations.Add(ValidacaoDocumento.Create(
            cenario.TenantId, documentoId, [], [], true, 1m, item.Id));
        await contexto.SaveChangesAsync();

        var resultado = await cenario.Servico.AprovarManualmenteAsync(
            new AprovarDocumentoAdmissaoRequisicao(cenario.AdmissaoId, item.Id, "Conferido visualmente"));

        Assert.False(resultado.Sucesso);
        Assert.Contains("reprovados", resultado.Erro);
    }

    [Fact]
    public async Task AprovarManualmente_NaoPermiteIgnorarTipoDeDocumentoIncompativel()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos
            .Include(documento => documento.TipoDocumento)
            .SingleAsync(documento => documento.TipoDocumento.Codigo == "CNH");
        await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, item.Id, "conta.pdf"));
        var documentoId = (await contexto.AdmissaoDocumentos.SingleAsync(registro => registro.Id == item.Id)).DocumentoId!.Value;
        contexto.DocumentValidations.Add(ValidacaoDocumento.Create(
            cenario.TenantId,
            documentoId,
            new Dictionary<string, string>
            {
                ["__tipo_documento_detectado"] = "COMPROVANTE_RESIDENCIA",
                ["__tipo_documento_incompativel"] = "true"
            },
            [], false, 0.98m, item.Id, observacao: "O documento nao corresponde ao tipo solicitado."));
        await contexto.SaveChangesAsync();

        var resultado = await cenario.Servico.AprovarManualmenteAsync(
            new AprovarDocumentoAdmissaoRequisicao(cenario.AdmissaoId, item.Id, "Aprovado visualmente"));

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao corresponde", resultado.Erro);
    }

    [Fact]
    public async Task Listar_NaoRetornaDocumentosDeOutroTenant()
    {
        var nomeBanco = Guid.NewGuid().ToString();
        var cenarioEstrangeiro = await CriarCenarioAsync(nomeBanco: nomeBanco);
        var admissaoId = cenarioEstrangeiro.AdmissaoId;
        await cenarioEstrangeiro.Contexto.DisposeAsync();

        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId, nomeBanco);
        var servico = CriarServicoDocumento(contexto, tenantId, new ArmazenamentoFake(), null);

        var resultado = await servico.ListarAsync(admissaoId);

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrada", resultado.Erro);
    }

    [Theory]
    [InlineData("arquivo.exe", "application/pdf")]
    [InlineData("arquivo.pdf", "image/png")]
    public async Task Enviar_QuandoExtensaoNaoCorrespondeAoMime_RejeitaArquivo(
        string nomeArquivo,
        string mimeType)
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        var conteudo = new byte[] { 1, 2, 3 };

        var resultado = await cenario.Servico.EnviarAsync(new(
            cenario.AdmissaoId, item.Id,
            [new(new MemoryStream(conteudo), nomeArquivo, mimeType, conteudo.Length)]));

        Assert.False(resultado.Sucesso);
        Assert.Contains("extensao", resultado.Erro);
        Assert.Empty(await contexto.Documents.ToListAsync());
    }

    private static EnviarDocumentoAdmissaoRequisicao CriarRequisicao(
        Guid admissaoId,
        Guid itemId,
        string nomeArquivo)
    {
        var conteudo = new byte[] { 37, 80, 68, 70, 45, 49, 46, 52 };
        return new(admissaoId, itemId,
            [new(new MemoryStream(conteudo), nomeArquivo, "application/pdf", conteudo.Length)]);
    }

    private static async Task<Cenario> CriarCenarioAsync(
        Resultado<ValidationResultDto>? resultadoValidacao = null,
        string? nomeBanco = null)
    {
        var tenantId = Guid.NewGuid();
        var contexto = CriarContexto(tenantId, nomeBanco);
        var empresa = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa Teste");
        var cliente = Cliente.CriarParaEmpresa(tenantId, empresa.Id);
        contexto.Empresas.Add(empresa);
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();

        var empresaAtual = CriarEmpresaAtual(tenantId);
        var repositorio = new AdmissaoRepositorio(contexto);
        var admissaoServico = new AdmissaoServico(repositorio, new ClienteRepositorio(contexto), empresaAtual);
        var dados = new DadosAdmissaoRequisicao(
            "Funcionario Teste", "11789299900", "1234567", new DateOnly(1995, 5, 10), "Feminino", null, null, null, null, null,
            null, null, null, null, new DateOnly(2026, 9, 22), 2500m, null, "Analista",
            90, false, null, null, []);
        var criacao = await admissaoServico.CriarManualAsync(new CriarAdmissaoManualRequisicao(cliente.Id, dados));
        var admissao = await contexto.Admissoes.SingleAsync(item => item.Id == criacao.Valor);
        var aguardandoValidacao = await repositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.AguardandoValidacao, "Aguardando validacao", 3);
        admissao.AlterarStatus(aguardandoValidacao.Id);
        await contexto.SaveChangesAsync();

        var armazenamento = new ArmazenamentoFake();
        var servico = CriarServicoDocumento(contexto, tenantId, armazenamento, resultadoValidacao);
        return new(contexto, servico, armazenamento, tenantId, RevisorId, criacao.Valor);
    }

    private static AdmissaoDocumentoServico CriarServicoDocumento(
        ContextoAplicacao contexto,
        Guid tenantId,
        ArmazenamentoFake armazenamento,
        Resultado<ValidationResultDto>? resultadoValidacao)
        => new(
            new AdmissaoRepositorio(contexto),
            new DocumentoServicoFake(resultadoValidacao),
            armazenamento,
            new EstimadorQualidadeFake(),
            new ConversorImagensPdfFake(),
            CriarEmpresaAtual(tenantId),
            new UsuarioAtualFake(RevisorId),
            NullLogger<AdmissaoDocumentoServico>.Instance);

    private static readonly Guid RevisorId = Guid.NewGuid();

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

    private sealed record Cenario(
        ContextoAplicacao Contexto,
        AdmissaoDocumentoServico Servico,
        ArmazenamentoFake Armazenamento,
        Guid TenantId,
        Guid RevisorId,
        Guid AdmissaoId);

    private sealed class UsuarioAtualFake(Guid userId) : IUsuarioAtual
    {
        public Guid UserId => userId;
        public string? Email => "revisor@officio.local";
    }

    private sealed class DocumentoServicoFake(Resultado<ValidationResultDto>? resultado) : IDocumentoServico
    {
        public Task<Resultado<ResultadoEnvioDocumentoDto>> EnviarAsync(EnviarDocumentoRequisicao requisicao, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<Resultado<ValidationResultDto>> ValidarAsync(Guid documentoId, CancellationToken ct = default)
            => Task.FromResult(resultado ?? Resultado<ValidationResultDto>.CriarSucesso(
                new ValidationResultDto(true, 1m, [], false, null)));
    }

    private sealed class EstimadorQualidadeFake : IEstimadorQualidadeDocumento
    {
        public Task<decimal> EstimarAsync(Stream arquivo, CancellationToken cancellationToken = default)
            => Task.FromResult(1m);
    }

    [Fact]
    public async Task Enviar_VariasImagens_ArmazenaUmPdfMultipagina()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        var arquivos = new byte[] { 1, 2, 3 };

        var resultado = await cenario.Servico.EnviarAsync(new(
            cenario.AdmissaoId,
            item.Id,
            [
                new(new MemoryStream(arquivos), "frente.jpg", "image/jpeg", arquivos.Length),
                new(new MemoryStream(arquivos), "verso.png", "image/png", arquivos.Length)
            ]));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal("application/pdf", resultado.Valor!.Documento.MimeType);
        Assert.EndsWith("-multipagina.pdf", resultado.Valor.Documento.NomeArquivo);
        Assert.Single(await contexto.Documents.ToListAsync());
    }

    [Fact]
    public async Task Enviar_CNHDepoisDaIdentidade_SubstituiAlternativaAnterior()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var identidade = await contexto.AdmissaoDocumentos
            .Include(item => item.TipoDocumento)
            .SingleAsync(item => item.TipoDocumento.Codigo == "IDENTIDADE");
        var cnh = await contexto.AdmissaoDocumentos
            .Include(item => item.TipoDocumento)
            .SingleAsync(item => item.TipoDocumento.Codigo == "CNH");

        await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, identidade.Id, "identidade.pdf"));
        var resultado = await cenario.Servico.EnviarAsync(CriarRequisicao(cenario.AdmissaoId, cnh.Id, "cnh.pdf"));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Single(await contexto.Documents.ToListAsync());
        Assert.Null((await contexto.AdmissaoDocumentos.SingleAsync(item => item.Id == identidade.Id)).DocumentoId);
        Assert.NotNull((await contexto.AdmissaoDocumentos.SingleAsync(item => item.Id == cnh.Id)).DocumentoId);
        Assert.Contains(cenario.Armazenamento.Excluidos, caminho => caminho.Contains("identidade.pdf"));
    }

    [Fact]
    public async Task Enviar_MisturaPdfEImagem_RejeitaComOrientacao()
    {
        var cenario = await CriarCenarioAsync();
        await using var contexto = cenario.Contexto;
        var item = await contexto.AdmissaoDocumentos.OrderBy(documento => documento.Ordem).FirstAsync();
        var arquivo = new byte[] { 1, 2, 3 };

        var resultado = await cenario.Servico.EnviarAsync(new(
            cenario.AdmissaoId,
            item.Id,
            [
                new(new MemoryStream(arquivo), "documento.pdf", "application/pdf", arquivo.Length),
                new(new MemoryStream(arquivo), "frente.jpg", "image/jpeg", arquivo.Length)
            ]));

        Assert.False(resultado.Sucesso);
        Assert.Contains("selecione somente imagens", resultado.Erro);
        Assert.Empty(await contexto.Documents.ToListAsync());
    }

    private sealed class ConversorImagensPdfFake : IConversorImagensPdf
    {
        public byte[] CriarPdf(IReadOnlyList<byte[]> imagens)
            => [37, 80, 68, 70, .. imagens.SelectMany(imagem => imagem)];
    }

    private sealed class ArmazenamentoFake : IServicoArmazenamento
    {
        public List<string> Excluidos { get; } = [];

        public Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
            => Task.FromResult($"{container}/{fileName}");

        public Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
            => Task.FromResult<Stream>(new MemoryStream());

        public Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default)
            => Task.FromResult($"https://storage.local/{path}");

        public Task ExcluirAsync(string path, CancellationToken ct = default)
        {
            Excluidos.Add(path);
            return Task.CompletedTask;
        }
    }
}
