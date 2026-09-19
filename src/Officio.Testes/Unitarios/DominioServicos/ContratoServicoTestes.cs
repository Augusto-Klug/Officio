using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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
    public async Task ObterPorIdAsync_ComPdfAssinado_InformaDisponibilidadeEDataAssinatura()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var dataAssinatura = new DateTime(2026, 8, 30, 10, 0, 0, DateTimeKind.Utc);
        contrato.MarkAsSigned(dataAssinatura);
        contexto.Documents.Add(Documento.CriarContratoAssinado(
            tenantId,
            contrato.Id,
            contrato.EmpresaId,
            contrato.ClientId,
            contrato.ProspectId,
            "contrato-assinado.pdf",
            "contratos/assinado.pdf",
            123));
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ObterPorIdAsync(contrato.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(resultado.Valor!.PdfAssinadoDisponivel);
        Assert.Equal(dataAssinatura, resultado.Valor.AssinadoEm);
    }

    [Fact]
    public async Task ObterPdfAssinadoAsync_ComDocumentoAssinado_RetornaUrlPreAssinada()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        contexto.Documents.Add(Documento.CriarContratoAssinado(
            tenantId,
            contrato.Id,
            contrato.EmpresaId,
            contrato.ClientId,
            contrato.ProspectId,
            "contrato-assinado.pdf",
            "contratos/assinado.pdf",
            123));
        await contexto.SaveChangesAsync();
        var armazenamento = new ServicoArmazenamentoFake();
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            new ServicoAssinaturaFake(),
            armazenamento);

        var resultado = await servico.ObterPdfAssinadoAsync(contrato.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal("contratos/assinado.pdf", resultado.Valor!.PdfUrl);
        Assert.Equal("contratos/assinado.pdf", armazenamento.UltimoCaminhoPreAssinado);
    }

    [Fact]
    public async Task ObterPdfAssinadoAsync_SemDocumentoAssinado_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ObterPdfAssinadoAsync(contrato.Id);

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
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
        contexto.Tenants.Add(empresa);
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
    public async Task ProcessarWebhookAutentiqueAsync_DocumentFinishedBaixaEArmazenaPdfAssinado()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var armazenamento = new ServicoArmazenamentoFake();
        var assinatura = new ServicoAssinaturaFake(
            documentoAssinado: ResultadoDocumentoAssinado.CriarSucesso(
                new MemoryStream([10, 20, 30]),
                "contrato-final.pdf"));
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            assinatura,
            armazenamento);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        var envio = Assert.Single(armazenamento.Envios);
        Assert.Equal("contracts", envio.Container);
        Assert.Equal([10, 20, 30], envio.Conteudo);
        Assert.Contains($"contratos/{contrato.Id}/assinaturas/contrato-final.pdf", envio.FileName);
        var documento = await contexto.Documents.SingleAsync();
        Assert.Equal(contrato.Id, documento.ContratoId);
        Assert.Equal("ASSINATURA_AUTENTIQUE", documento.Origem);
        Assert.Equal("application/pdf", documento.MimeType);
        Assert.Equal("contrato-final.pdf", documento.NomeArquivo);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_FalhaNoDownloadMantemContratoAssinadoPersistido()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var assinatura = new ServicoAssinaturaFake(
            documentoAssinado: ResultadoDocumentoAssinado.CriarFalha("PDF ainda indisponivel."));
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            assinatura);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.False(resultado.Sucesso);
        contexto.ChangeTracker.Clear();
        var contratoPersistido = await contexto.Contracts.SingleAsync(item => item.Id == contrato.Id);
        Assert.Equal(StatusContrato.Ativo, contratoPersistido.Status);
        Assert.NotNull(contratoPersistido.AssinadoEm);
        Assert.Single(await contexto.AssinaturaEventos.ToListAsync());
        Assert.Empty(await contexto.Documents.ToListAsync());
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_DocumentFinishedComEnvelopeAutentique_AtivaContrato()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {
              "id": "webhook-1",
              "object": "webhook",
              "event": {
                "id": "event-1",
                "object": "event",
                "type": "document.finished",
                "data": {
                  "object": {
                    "id": "doc-autentique-1",
                    "object": "document"
                  }
                },
                "created_at": "2026-08-30T10:00:00Z"
              }
            }
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
        Assert.NotNull(contrato.AssinadoEm);
        Assert.Single(contexto.Documents);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_DocumentUpdatedFinalizadoComDataDocumento_AtivaContrato()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var armazenamento = new ServicoArmazenamentoFake();
        var assinatura = new ServicoAssinaturaFake(
            documentoAssinado: ResultadoDocumentoAssinado.CriarSucesso(
                new MemoryStream([10, 20, 30]),
                "contrato-final.pdf"));
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            assinatura,
            armazenamento);
        var payload = """
            {
              "event": {
                "type": "document.updated",
                "data": {
                  "id": "doc-autentique-1",
                  "object": "document",
                  "files": {
                    "signed": "https://app.autentique.com.br/documentos/doc-autentique-1/assinado.pdf"
                  },
                  "signatures_count": 1,
                  "signed_count": 1
                },
                "created_at": "2026-08-30T10:00:00Z"
              }
            }
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
        Assert.NotNull(contrato.AssinadoEm);
        Assert.Single(contexto.Documents);
        Assert.Single(armazenamento.Envios);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_SemTenantNoRequest_CorrelacionaSolicitacaoEAtivaContrato()
    {
        var tenantId = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        await using var contexto = CriarContexto(tenantId, empresaAtual);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        empresaAtual.SetTenant(Guid.Empty);
        var servico = CriarServico(contexto, tenantId, new ServicoPdfFake(), new ServicoAssinaturaFake(), null, empresaAtual);
        var payload = """
            {
              "event": {
                "type": "document.finished",
                "data": {
                  "id": "doc-autentique-1",
                  "object": "document"
                },
                "created_at": "2026-08-30T10:00:00Z"
              }
            }
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(tenantId, empresaAtual.TenantId);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
        Assert.Single(contexto.Documents);
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

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_QuandoSolicitacaoNaoExiste_IgnoraComSucesso()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-inexistente"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Empty(contexto.AssinaturaEventos);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_EventoDesconhecido_RegistraEventoSemAlterarStatus()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, solicitacao) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {"event":"document.viewed","document":{"id":"doc-autentique-1"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.Rascunho, contrato.Status);
        Assert.Null(solicitacao.ConcluidoEm);
        var evento = Assert.Single(contexto.AssinaturaEventos);
        Assert.Equal("document.viewed", evento.TipoEvento);
        Assert.Equal(new DateTime(2026, 8, 30, 10, 0, 0, DateTimeKind.Utc), evento.OcorridoEm);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_SignatureAcceptedComPayloadAninhado_MarcaSignatario()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, _, solicitacao) = await CriarContratoComSolicitacaoAsync(
            contexto,
            tenantId,
            incluirSignatario: true);
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {
              "data": {
                "type": "signature.accepted",
                "object": {
                  "document": { "uuid": "doc-autentique-1" },
                  "signer": { "email": "cliente@teste.com" }
                },
                "occurredAt": "2026-08-30T11:15:00Z"
              }
            }
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        var signatario = Assert.Single(solicitacao.Signatarios);
        Assert.Equal(new DateTime(2026, 8, 30, 11, 15, 0, DateTimeKind.Utc), signatario.AssinadoEm);
        Assert.Single(contexto.AssinaturaEventos);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_ComCamposObjetoOuString_NaoLancaExcecao()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, _, solicitacao) = await CriarContratoComSolicitacaoAsync(
            contexto,
            tenantId,
            incluirSignatario: true);
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {
              "event": { "name": "signature.accepted" },
              "object": "signature",
              "data": {
                "document": "doc-autentique-1",
                "signer": { "email": "cliente@teste.com" },
                "created_at": "2026-08-30T11:15:00Z"
              }
            }
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(new DateTime(2026, 8, 30, 11, 15, 0, DateTimeKind.Utc), Assert.Single(solicitacao.Signatarios).AssinadoEm);
        Assert.Single(contexto.AssinaturaEventos);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_SignatureAcceptedComEnvelopeAutentique_MarcaSignatario()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, _, solicitacao) = await CriarContratoComSolicitacaoAsync(
            contexto,
            tenantId,
            incluirSignatario: true);
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {
              "id": "webhook-1",
              "object": "webhook",
              "event": {
                "id": "event-1",
                "object": "event",
                "type": "signature.accepted",
                "data": {
                  "public_id": "signature-1",
                  "object": "signature",
                  "user": {
                    "email": "cliente@teste.com"
                  },
                  "document": "doc-autentique-1",
                  "signed": "2026-08-30T11:15:00Z"
                },
                "created_at": "2026-08-30T11:15:00Z"
              }
            }
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(new DateTime(2026, 8, 30, 11, 15, 0, DateTimeKind.Utc), Assert.Single(solicitacao.Signatarios).AssinadoEm);
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
        IServicoAssinatura servicoAssinatura,
        IServicoArmazenamento? servicoArmazenamento = null,
        ServicoEmpresaAtual? empresaAtual = null)
        => new ContratoServico(
            new ContratoRepositorio(contexto),
            new EmpresaRepositorio(contexto),
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto),
            servicoPdf,
            servicoAssinatura,
            servicoArmazenamento ?? new ServicoArmazenamentoFake(),
            empresaAtual ?? CriarEmpresaAtual(tenantId),
            NullLogger<ContratoServico>.Instance);

    private static ContextoAplicacao CriarContexto(Guid tenantId)
        => CriarContexto(tenantId, CriarEmpresaAtual(tenantId));

    private static ContextoAplicacao CriarContexto(Guid tenantId, ServicoEmpresaAtual empresaAtual)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return empresaAtual;
    }

    private static async Task<(Prospecto Prospecto, Contrato Contrato, AssinaturaSolicitacao Solicitacao)> CriarContratoComSolicitacaoAsync(
        ContextoAplicacao contexto,
        Guid tenantId,
        bool incluirSignatario = false)
    {
        var prospecto = Prospecto.Create(tenantId, "Empresa Prospect", "11222333000144", null);
        var cliente = Cliente.CreateFromProspect(prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, new DateTime(2026, 9, 1), null, true, null);
        var status = new StatusAssinatura("aguardando_assinatura", "Aguardando assinatura", 10);
        var solicitacao = AssinaturaSolicitacao.Criar(tenantId, contrato.Id, status.Id, "autentique");
        solicitacao.DefinirIdentificadorExterno("doc-autentique-1");
        if (incluirSignatario)
            solicitacao.AdicionarSignatario("Cliente Teste", "cliente@teste.com", "cliente", 1, "12345678901");

        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        contexto.StatusAssinaturas.Add(status);
        contexto.AssinaturaSolicitacoes.Add(solicitacao);
        await contexto.SaveChangesAsync();

        return (prospecto, contrato, solicitacao);
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
        ResultadoAssinatura? resultado = null,
        ResultadoDocumentoAssinado? documentoAssinado = null) : IServicoAssinatura
    {
        public Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(SolicitacaoAssinatura request, CancellationToken ct = default)
            => Task.FromResult(resultado ?? ResultadoAssinatura.CriarFalha("nao usado"));

        public Task<ResultadoDocumentoAssinado> BaixarDocumentoAssinadoAsync(string documentoId, CancellationToken ct = default)
            => Task.FromResult(documentoAssinado ?? ResultadoDocumentoAssinado.CriarSucesso(
                new MemoryStream([37, 80, 68, 70]),
                "contrato-assinado.pdf"));
    }

    private sealed class ServicoArmazenamentoFake : IServicoArmazenamento
    {
        public List<EnvioArmazenamento> Envios { get; } = [];
        public string? UltimoCaminhoPreAssinado { get; private set; }

        public async Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
        {
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria, ct);
            Envios.Add(new EnvioArmazenamento(fileName, container, memoria.ToArray()));

            return fileName;
        }

        public Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
            => Task.FromResult<Stream>(new MemoryStream());

        public Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default)
        {
            UltimoCaminhoPreAssinado = path;
            return Task.FromResult(path);
        }
    }

    private sealed record EnvioArmazenamento(string FileName, string Container, byte[] Conteudo);
}
