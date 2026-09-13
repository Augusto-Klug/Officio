using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Officio.Dominio.Contratos;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class ContratoServicoTestes
{
    [Fact]
    public async Task ConverterOrcamentoAsync_ComOrcamentoAceito_CriaContrato()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 500m, 1));
        orcamento.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            orcamento.Id,
            new DateTime(2026, 9, 1),
            null,
            true,
            null));

        Assert.True(resultado.Sucesso);
        var contrato = await contexto.Contracts.SingleAsync();
        Assert.Equal(prospecto.Id, contrato.ProspectId);
        Assert.Null(contrato.ClientId);
        Assert.Equal(orcamento.Id, contrato.BudgetId);
    }

    [Fact]
    public async Task ConverterOrcamentoAsync_ComOrcamentoRascunho_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            orcamento.Id,
            DateTime.UtcNow,
            null,
            true,
            null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("aceitos", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListarAsync_ComContratoVinculadoAoCliente_RetornaRazaoSocialDoCliente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Prospecto Original", "11222333000144", null);
        var cliente = Cliente.CreateFromProspect(
            Prospecto.Create(tenantId, "Cliente Contratante", "99888777000166", null));
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var contrato = Contrato.CreateFromBudget(
            orcamento,
            cliente.Id,
            new DateTime(2026, 9, 1),
            null,
            true,
            null);
        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ListarAsync();

        Assert.True(resultado.Sucesso);
        var contratoResultado = Assert.Single(resultado.Valor!);
        Assert.Equal("Cliente Contratante", contratoResultado.RazaoSocial);
    }

    [Fact]
    public async Task GerarPdfAsync_ComContratoVinculadoAoProspecto_UsaDadosDoProspecto()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var empresa = Empresa.Create("Contabilidade Teste", "00999888000177");
        DefinirId(empresa, tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Prospect", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Honorarios",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1));
        orcamento.Aceitar();
        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 9, 1),
            null,
            true,
            null);
        contexto.Empresas.Add(empresa);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var pdf = new ServicoPdfCapturaFake();
        var servico = CriarServico(contexto, tenantId, pdf);

        var resultado = await servico.GerarPdfAsync(contrato.Id);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Empresa Prospect", pdf.UltimosDados!.Contratante.RazaoSocial);
        Assert.Equal("11222333000144", pdf.UltimosDados.Contratante.Cnpj);
    }

    [Fact]
    public async Task EnviarParaAssinaturaAsync_ComAutentiqueCriaSolicitacaoAssinatura()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Prospect", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.Aceitar();
        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 9, 1),
            null,
            true,
            null);
        contrato.SetPdfPath("contratos/contrato.pdf");
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            new ServicoAssinaturaFake(ResultadoAssinatura.CriarSucesso(
                "doc-autentique-1",
                [new LinkAssinante("cliente@teste.com", "https://assinatura.test")])));

        var resultado = await servico.EnviarParaAssinaturaAsync(new EnviarAssinaturaRequisicao(
            contrato.Id,
            [new SignatarioAssinatura("Cliente Teste", "cliente@teste.com", "12345678901", "cliente")]));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.AguardandoAssinatura, contrato.Status);
        var solicitacao = await contexto.AssinaturaSolicitacoes
            .Include(item => item.Signatarios)
            .SingleAsync();
        Assert.Equal("autentique", solicitacao.Provedor);
        Assert.Equal("doc-autentique-1", solicitacao.IdExterno);
        Assert.Equal("cliente@teste.com", Assert.Single(solicitacao.Signatarios).Email);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_DocumentFinishedAtivaContratoERegistraEvento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Prospect", "11222333000144", null);
        var cliente = Cliente.CreateFromProspect(prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, new DateTime(2026, 9, 1), null, true, null);
        var status = new StatusAssinatura("aguardando_assinatura", "Aguardando assinatura", 10);
        var solicitacao = AssinaturaSolicitacao.Criar(tenantId, contrato.Id, status.Id, "autentique");
        solicitacao.DefinirIdentificadorExterno("doc-autentique-1");
        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        contexto.StatusAssinaturas.Add(status);
        contexto.AssinaturaSolicitacoes.Add(solicitacao);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
        Assert.NotNull(contrato.AssinadoEm);
        Assert.Single(contexto.AssinaturaEventos);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_EventoDuplicadoNaoRegistraNovamente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Prospect", "11222333000144", null);
        var cliente = Cliente.CreateFromProspect(prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, new DateTime(2026, 9, 1), null, true, null);
        var status = new StatusAssinatura("aguardando_assinatura", "Aguardando assinatura", 10);
        var solicitacao = AssinaturaSolicitacao.Criar(tenantId, contrato.Id, status.Id, "autentique");
        solicitacao.DefinirIdentificadorExterno("doc-autentique-1");
        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        contexto.StatusAssinaturas.Add(status);
        contexto.AssinaturaSolicitacoes.Add(solicitacao);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        await servico.ProcessarWebhookAutentiqueAsync(payload);
        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Single(contexto.AssinaturaEventos);
    }

    private static IContratoServico CriarServico(ContextoAplicacao contexto, Guid tenantId)
        => CriarServico(contexto, tenantId, new ServicoPdfFake());

    private static IContratoServico CriarServico(ContextoAplicacao contexto, Guid tenantId, IServicoPdf servicoPdf)
        => CriarServico(contexto, tenantId, servicoPdf, new ServicoAssinaturaFake());

    private static IContratoServico CriarServico(
        ContextoAplicacao contexto,
        Guid tenantId,
        IServicoPdf servicoPdf,
        IServicoAssinatura servicoAssinatura)
        => new ContratoServico(
            new ContratoRepositorio(contexto),
            new EmpresaRepositorio(contexto),
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto),
            servicoPdf,
            servicoAssinatura,
            new ServicoArmazenamentoFake(),
            CriarEmpresaAtual(tenantId));

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, CriarEmpresaAtual(tenantId), new PublicadorEventosFake());
    }

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return empresaAtual;
    }

    private static void DefinirId(EntidadeBase entidade, Guid id)
    {
        typeof(EntidadeBase)
            .GetProperty(nameof(EntidadeBase.Id), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(entidade, id);
    }

    private sealed class ServicoPdfFake : IServicoPdf
    {
        public Task<Stream> GerarContratoAsync(DadosPdfContrato data, CancellationToken ct = default)
            => Task.FromResult<Stream>(new MemoryStream());
    }

    private sealed class ServicoPdfCapturaFake : IServicoPdf
    {
        public DadosPdfContrato? UltimosDados { get; private set; }

        public Task<Stream> GerarContratoAsync(DadosPdfContrato data, CancellationToken ct = default)
        {
            UltimosDados = data;
            return Task.FromResult<Stream>(new MemoryStream());
        }
    }

    private sealed class ServicoAssinaturaFake(
        ResultadoAssinatura? resultado = null) : IServicoAssinatura
    {
        public Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(SolicitacaoAssinatura request, CancellationToken ct = default)
            => Task.FromResult(resultado ?? ResultadoAssinatura.CriarFalha("nao usado"));
    }

    private sealed class ServicoArmazenamentoFake : IServicoArmazenamento
    {
        public Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
            => Task.FromResult(fileName);

        public Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
            => Task.FromResult<Stream>(new MemoryStream());

        public Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default)
            => Task.FromResult(path);
    }
}
