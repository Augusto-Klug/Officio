using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;
using Officio.Testes.Dubles;

namespace Officio.Testes.Unitarios.DominioServicos;

public class DocumentoServicoTestes
{
    [Fact]
    public async Task EnviarAsync_QuandoArquivoExcedeDezMegabytes_RejeitaAntesDoArmazenamento()
    {
        var tenantId = Guid.NewGuid();
        await using var provedor = CriarProvedor(tenantId, ResultadoOcr.CriarFalha("nao usado"));
        var servico = provedor.GetRequiredService<IDocumentoServico>();
        var armazenamento = provedor.GetRequiredService<ServicoArmazenamentoFake>();
        await using var arquivo = new MemoryStream(new byte[(10 * 1024 * 1024) + 1]);

        var resultado = await servico.EnviarAsync(new EnviarDocumentoRequisicao(
            arquivo,
            "documento-grande.bmp",
            TipoDocumento.CartaoCnpj,
            ClienteId: Guid.NewGuid(),
            ProspectoId: null,
            SocioId: null,
            Observacoes: null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("10 MB", resultado.Erro);
        Assert.Empty(armazenamento.ConteudoEnviado);
    }

    [Fact]
    public async Task EnviarAsync_QuandoArquivoSeekableEstaNoFim_ArmazenaConteudoCompleto()
    {
        var tenantId = Guid.NewGuid();
        var conteudo = new byte[] { 50, 60, 70, 80 };
        await using var provedor = CriarProvedor(tenantId, ResultadoOcr.CriarFalha("nao usado"));
        var servico = provedor.GetRequiredService<IDocumentoServico>();
        var armazenamento = provedor.GetRequiredService<ServicoArmazenamentoFake>();
        using var arquivo = new MemoryStream(conteudo);
        arquivo.Position = arquivo.Length;

        var resultado = await servico.EnviarAsync(new EnviarDocumentoRequisicao(
            arquivo,
            "documento.jpg",
            TipoDocumento.CartaoCnpj,
            ClienteId: Guid.NewGuid(),
            ProspectoId: null,
            SocioId: null,
            Observacoes: null));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(conteudo, armazenamento.ConteudoEnviado);
    }

    [Fact]
    public async Task EnviarAsync_QuandoArquivoNaoPermiteReposicionamento_ArmazenaConteudoCompleto()
    {
        var tenantId = Guid.NewGuid();
        var conteudo = new byte[] { 10, 20, 30, 40 };
        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarFalha("nao usado"),
            consumirArquivoNaEstimativa: true);
        var servico = provedor.GetRequiredService<IDocumentoServico>();
        var armazenamento = provedor.GetRequiredService<ServicoArmazenamentoFake>();

        var resultado = await servico.EnviarAsync(new EnviarDocumentoRequisicao(
            new StreamNaoSeekable(conteudo),
            "documento.jpg",
            TipoDocumento.CartaoCnpj,
            ClienteId: Guid.NewGuid(),
            ProspectoId: null,
            SocioId: null,
            Observacoes: null));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(conteudo, armazenamento.ConteudoEnviado);
    }

    [Fact]
    public async Task EnviarAsync_QuandoQualidadeAbaixoDoLimite_RejeitaDocumento()
    {
        var tenantId = Guid.NewGuid();
        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarFalha("nao usado"),
            qualidadeDocumento: 0.2m);
        var servico = provedor.GetRequiredService<IDocumentoServico>();

        var resultado = await servico.EnviarAsync(new EnviarDocumentoRequisicao(
            new MemoryStream([0, 0, 0, 0]),
            "documento-escuro.jpg",
            TipoDocumento.CartaoCnpj,
            ClienteId: Guid.NewGuid(),
            ProspectoId: null,
            SocioId: null,
            Observacoes: null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("Imagem muito escura ou desfocada", resultado.Erro);
    }

    [Fact]
    public async Task ValidarAsync_QuandoDocumentoDivergeDoCadastro_RegistraDivergencia()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, cliente) = CriarEmpresaECliente(tenantId, "Empresa Certa Ltda", "11222333000144");
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.85m, clientId: cliente.Id);

        await using var provedor = CriarProvedor(tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["razao_social"] = "Empresa Errada Ltda",
                ["cnpj"] = "11222333000144"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.Clients.Add(cliente);
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();

        var servico = provedor.GetRequiredService<IDocumentoServico>();

        var resultado = await servico.ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.False(resultado.Valor!.Valido);
        var divergencia = Assert.Single(resultado.Valor.Divergencias);
        Assert.Equal("razao_social", divergencia.Campo);
        Assert.Equal("Empresa Certa Ltda", divergencia.ValorCadastro);

        contexto.ChangeTracker.Clear();
        var execucao = await contexto.OcrExecucoes.Include(item => item.Campos).SingleAsync();
        var validacao = await contexto.DocumentValidations.Include(item => item.Divergencias).SingleAsync();
        Assert.Equal("CONCLUIDO", execucao.StatusProcessamento);
        Assert.Equal(2, execucao.Campos.Count);
        Assert.Equal(execucao.Id, validacao.OcrExecucaoId);
        Assert.Single(validacao.Divergencias);
    }

    [Fact]
    public async Task ValidarAsync_QuandoDocumentoDeProspectoDivergeDoCadastro_RegistraDivergencia()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Prospecto Certa Ltda", "99888777000166", null);
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.85m, prospectId: prospecto.Id);

        await using var provedor = CriarProvedor(tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["razao_social"] = "Prospecto Errado Ltda",
                ["cnpj"] = "99888777000166"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Prospects.Add(prospecto);
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();

        var servico = provedor.GetRequiredService<IDocumentoServico>();

        var resultado = await servico.ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.False(resultado.Valor!.Valido);
        var divergencia = Assert.Single(resultado.Valor.Divergencias);
        Assert.Equal("razao_social", divergencia.Campo);
        Assert.Equal("Prospecto Certa Ltda", divergencia.ValorCadastro);
    }

    [Fact]
    public async Task ValidarAsync_QuandoDocumentoDeProspectoConfere_Aprova()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Prospecto Certa Ltda", "99888777000166", null);
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.85m, prospectId: prospecto.Id);

        await using var provedor = CriarProvedor(tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["razao_social"] = "Prospecto Certa Ltda",
                ["cnpj"] = "99888777000166"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Prospects.Add(prospecto);
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();

        var servico = provedor.GetRequiredService<IDocumentoServico>();

        var resultado = await servico.ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.Valido);
        Assert.Empty(resultado.Valor.Divergencias);

        contexto.ChangeTracker.Clear();
        var validacao = await contexto.DocumentValidations.SingleAsync();
        Assert.Equal(tenantId, validacao.TenantId);
    }

    [Fact]
    public async Task ValidarAsync_QuandoSimilaridadeAcimaDoLimite_Aprova()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, cliente) = CriarEmpresaECliente(tenantId, "Empresa Certa Ltda", "11222333000144");
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.85m, clientId: cliente.Id);

        await using var provedor = CriarProvedor(tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["razao_social"] = "Empresa Certa Ltda",
                ["cnpj"] = "11222333000144"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.Clients.Add(cliente);
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();

        var servico = provedor.GetRequiredService<IDocumentoServico>();

        var resultado = await servico.ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.Valido);
        Assert.Empty(resultado.Valor.Divergencias);
    }

    [Fact]
    public async Task ValidarAsync_QuandoOcrIndisponivel_SegueParaRevisaoManual()
    {
        var tenantId = Guid.NewGuid();
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.85m, clientId: Guid.NewGuid());

        await using var provedor = CriarProvedor(tenantId, ResultadoOcr.CriarFalha("indisponivel"));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();

        var servico = provedor.GetRequiredService<IDocumentoServico>();

        var resultado = await servico.ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.FallbackManual);
        Assert.False(resultado.Valor.Valido);

        contexto.ChangeTracker.Clear();
        var validacao = await contexto.DocumentValidations.SingleAsync();
        Assert.Equal(tenantId, validacao.TenantId);
        var execucao = await contexto.OcrExecucoes.SingleAsync();
        Assert.Equal("FALHA", execucao.StatusProcessamento);
        Assert.Equal(execucao.Id, validacao.OcrExecucaoId);
    }

    [Fact]
    public async Task ValidarAsync_QuandoCadastroNaoExiste_SegueParaRevisaoManual()
    {
        var tenantId = Guid.NewGuid();
        var documento = Documento.Create(
            tenantId,
            "cartao-cnpj.pdf",
            TipoDocumento.CartaoCnpj,
            "doc.pdf",
            0.85m,
            clientId: Guid.NewGuid());

        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["razao_social"] = "Empresa Certa Ltda",
                ["cnpj"] = "11222333000144"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.FallbackManual);
        Assert.False(resultado.Valor.Valido);
        Assert.Contains("cadastro", resultado.Valor.Mensagem, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidarAsync_QuandoCampoEsperadoNaoExisteNoOcr_SegueParaRevisaoManual()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, cliente) = CriarEmpresaECliente(tenantId, "Empresa Certa Ltda", "11222333000144");
        var documento = Documento.Create(
            tenantId,
            "cartao-cnpj.pdf",
            TipoDocumento.CartaoCnpj,
            "doc.pdf",
            0.85m,
            clientId: cliente.Id);

        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["cnpj"] = "11222333000144"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.Clients.Add(cliente);
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.FallbackManual);
        Assert.False(resultado.Valor.Valido);
        var divergencia = Assert.Single(resultado.Valor.Divergencias);
        Assert.Equal("razao_social", divergencia.Campo);
        Assert.Equal(string.Empty, divergencia.ValorDocumento);
        Assert.Equal(0m, divergencia.Similaridade);
    }

    [Fact]
    public async Task ValidarAsync_QuandoDocumentoPertenceAAdmissao_VinculaValidacaoAoChecklist()
    {
        var tenantId = Guid.NewGuid();
        var empresa = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa Teste");
        var status = new StatusAdmissao("RASCUNHO", "Rascunho", 1);
        var tipo = new TipoDocumentoDominio("IDENTIDADE", "Documento de identidade", 1);
        var admissao = Admissao.Criar(tenantId, empresa.Id, status.Id, "Ana Silva");
        admissao.AtualizarDados(
            "Ana Silva", "11789299900", "1234567", new DateOnly(1995, 5, 10), null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
        var item = AdmissaoDocumento.Criar(admissao.Id, tipo.Id, true, 1);
        var documento = Documento.CriarAdmissao(
            tenantId, admissao.Id, empresa.Id, tipo.Id, "identidade.pdf", TipoDocumento.Identidade,
            "documentos/identidade.pdf", "application/pdf", 100, 1m);
        item.VincularDocumento(documento);
        admissao.AdicionarDocumentos([item]);

        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["nome"] = "Ana Silva",
                ["cpf"] = "11789299900",
                ["rg"] = "1234567",
                ["data_nascimento"] = "1995-05-10"
            }, 0.98m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.StatusAdmissoes.Add(status);
        contexto.TiposDocumento.Add(tipo);
        contexto.Admissoes.Add(admissao);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        var validacao = await contexto.DocumentValidations.SingleAsync();
        Assert.Equal(item.Id, validacao.AdmissaoDocumentoId);
    }

    [Fact]
    public async Task ValidarAsync_ComprovanteEscolaridadeComNomeCorreto_NaoExigeCpfRgOuNascimento()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, status, tipo, admissao, documento) = CriarDocumentoAdmissao(
            tenantId,
            "COMPROVANTE_ESCOLARIDADE",
            TipoDocumento.ComprovanteEscolaridade,
            "escolaridade.pdf");

        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["nome"] = "Ana Silva",
                ["tipo_documento_detectado"] = "COMPROVANTE_ESCOLARIDADE"
            }, 0.96m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.StatusAdmissoes.Add(status);
        contexto.TiposDocumento.Add(tipo);
        contexto.Admissoes.Add(admissao);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.Valido);
        Assert.False(resultado.Valor.FallbackManual);
        Assert.Empty(resultado.Valor.Divergencias);
    }

    [Fact]
    public async Task ValidarAsync_TituloEleitoralComNomeEInscricaoNaoExigeCpf()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, status, tipo, admissao, documento) = CriarDocumentoAdmissao(
            tenantId,
            "TITULO_ELEITOR",
            TipoDocumento.TituloEleitor,
            "titulo-eleitoral.pdf");

        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["nome"] = "Ana Silva",
                ["numero_titulo"] = "067278480957",
                ["tipo_documento_detectado"] = "TITULO_ELEITOR"
            }, 0.99m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.StatusAdmissoes.Add(status);
        contexto.TiposDocumento.Add(tipo);
        contexto.Admissoes.Add(admissao);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.Valido);
        Assert.False(resultado.Valor.FallbackManual);
        Assert.Empty(resultado.Valor.Divergencias);
    }

    [Fact]
    public async Task ValidarAsync_TituloEleitoralSemInscricaoSegueParaRevisaoManual()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, status, tipo, admissao, documento) = CriarDocumentoAdmissao(
            tenantId,
            "TITULO_ELEITOR",
            TipoDocumento.TituloEleitor,
            "titulo-eleitoral.pdf");

        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["nome"] = "Ana Silva",
                ["tipo_documento_detectado"] = "TITULO_ELEITOR"
            }, 0.99m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.StatusAdmissoes.Add(status);
        contexto.TiposDocumento.Add(tipo);
        contexto.Admissoes.Add(admissao);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.FallbackManual);
        Assert.Contains("inscricao", resultado.Valor.Mensagem, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidarAsync_CnhSemCpf_SegueParaRevisaoManual()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, status, tipo, admissao, documento) = CriarDocumentoAdmissao(
            tenantId,
            "CNH",
            TipoDocumento.Cnh,
            "cnh.pdf");

        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["nome"] = "Ana Silva",
                ["tipo_documento_detectado"] = "CNH"
            }, 0.96m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.StatusAdmissoes.Add(status);
        contexto.TiposDocumento.Add(tipo);
        contexto.Admissoes.Add(admissao);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.FallbackManual);
        var divergencia = Assert.Single(resultado.Valor.Divergencias);
        Assert.Equal("cpf", divergencia.Campo);
    }

    [Fact]
    public async Task ValidarAsync_ComprovanteResidenciaNoCampoCnh_ReprovaSemPermitirRevisaoManual()
    {
        var tenantId = Guid.NewGuid();
        var (empresa, status, tipo, admissao, documento) = CriarDocumentoAdmissao(
            tenantId, "CNH", TipoDocumento.Cnh, "conta-luz.pdf");
        await using var provedor = CriarProvedor(
            tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["nome"] = "Ana Silva",
                ["cpf"] = "11789299900",
                ["endereco"] = "Rua das Flores, 123",
                ["tipo_documento_detectado"] = "COMPROVANTE_RESIDENCIA"
            }, 0.98m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        contexto.Empresas.Add(empresa);
        contexto.StatusAdmissoes.Add(status);
        contexto.TiposDocumento.Add(tipo);
        contexto.Admissoes.Add(admissao);
        await contexto.SaveChangesAsync();

        var resultado = await provedor.GetRequiredService<IDocumentoServico>().ValidarAsync(documento.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.False(resultado.Valor!.Valido);
        Assert.False(resultado.Valor.FallbackManual);
        Assert.Contains("nao corresponde", resultado.Valor.Mensagem, StringComparison.OrdinalIgnoreCase);
        contexto.ChangeTracker.Clear();
        var validacao = await contexto.DocumentValidations.SingleAsync();
        Assert.True(validacao.TipoDocumentoIncompativel);
        Assert.Equal("COMPROVANTE_RESIDENCIA", validacao.TipoDocumentoDetectado);
    }

    private static (Empresa Empresa, StatusAdmissao Status, TipoDocumentoDominio Tipo,
        Admissao Admissao, Documento Documento) CriarDocumentoAdmissao(
        Guid tenantId,
        string codigoTipo,
        TipoDocumento tipoDocumento,
        string nomeArquivo)
    {
        var empresa = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa Teste");
        var status = new StatusAdmissao("RASCUNHO", "Rascunho", 1);
        var tipo = new TipoDocumentoDominio(codigoTipo, codigoTipo, 1);
        var admissao = Admissao.Criar(tenantId, empresa.Id, status.Id, "Ana Silva");
        admissao.AtualizarDados(
            "Ana Silva", "11789299900", "1234567", new DateOnly(1995, 5, 10), null, null,
            "Ensino medio", null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null);
        var item = AdmissaoDocumento.Criar(admissao.Id, tipo.Id, true, 1);
        var documento = Documento.CriarAdmissao(
            tenantId, admissao.Id, empresa.Id, tipo.Id, nomeArquivo, tipoDocumento,
            $"documentos/{nomeArquivo}", "application/pdf", 100, 1m);
        item.VincularDocumento(documento);
        admissao.AdicionarDocumentos([item]);
        return (empresa, status, tipo, admissao, documento);
    }

    private static ServiceProvider CriarProvedor(
        Guid tenantId,
        ResultadoOcr resultadoOcr,
        decimal qualidadeDocumento = 0.85m,
        bool consumirArquivoNaEstimativa = false)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);

        var servicos = new ServiceCollection();
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<IServicoOcr>(new ServicoOcrFake(resultadoOcr));
        servicos.AddSingleton<ServicoArmazenamentoFake>();
        servicos.AddSingleton<IServicoArmazenamento>(sp => sp.GetRequiredService<ServicoArmazenamentoFake>());
        servicos.AddSingleton<PublicadorEventosFake>();
        servicos.AddSingleton<MediatR.IPublisher>(sp => sp.GetRequiredService<PublicadorEventosFake>());
        servicos.AddDbContext<ContextoAplicacao>(opcoes => opcoes.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        servicos.AddScoped<DbContext>(sp => sp.GetRequiredService<ContextoAplicacao>());
        servicos.AddScoped<IDocumentoRepositorio, DocumentoRepositorio>();
        servicos.AdicionarServicosDominio(new ConfigurationBuilder().Build());
        servicos.AddSingleton<IEstimadorQualidadeDocumento>(
            new EstimadorQualidadeDocumentoFake(qualidadeDocumento, consumirArquivoNaEstimativa));

        return servicos.BuildServiceProvider();
    }

    private static (Empresa Empresa, Cliente Cliente) CriarEmpresaECliente(
        Guid tenantId,
        string razaoSocial,
        string cnpj)
    {
        var prospecto = Prospecto.Create(tenantId, razaoSocial, cnpj, null);
        var empresa = Empresa.CriarDoProspecto(prospecto);
        prospecto.VincularEmpresa(empresa.Id);
        return (empresa, Cliente.CreateFromProspect(prospecto));
    }

    private sealed class ServicoOcrFake(ResultadoOcr resultado) : IServicoOcr
    {
        public Task<ResultadoOcr> AnalisarDocumentoAsync(Stream documentStream, TipoDocumento type, CancellationToken ct = default)
            => Task.FromResult(resultado);
    }

    private sealed class EstimadorQualidadeDocumentoFake(
        decimal qualidade,
        bool consumirArquivo) : IEstimadorQualidadeDocumento
    {
        public async Task<decimal> EstimarAsync(Stream arquivo, CancellationToken cancellationToken = default)
        {
            if (consumirArquivo)
                await arquivo.CopyToAsync(Stream.Null, cancellationToken);

            return qualidade;
        }
    }

    private sealed class ServicoArmazenamentoFake : IServicoArmazenamento
    {
        public byte[] ConteudoEnviado { get; private set; } = [];

        public async Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
        {
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria, ct);
            ConteudoEnviado = memoria.ToArray();

            return fileName;
        }

        public Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
            => Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));

        public Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default)
            => Task.FromResult(path);

        public Task ExcluirAsync(string path, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class StreamNaoSeekable(byte[] conteudo) : Stream
    {
        private readonly MemoryStream memoria = new(conteudo);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => memoria.Flush();
        public override int Read(byte[] buffer, int offset, int count) => memoria.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
