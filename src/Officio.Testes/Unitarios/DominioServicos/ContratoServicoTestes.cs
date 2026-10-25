using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using Officio.Dominio.Contratos;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;
using Officio.Dominio.Contratos.Response;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Unitarios.DominioServicos;

public class ContratoServicoTestes
{
    [Fact]
    public async Task ContratoLegado_PermaneceSemRepresentanteAposPersistencia()
    {
        var tenant = Guid.NewGuid(); await using var db = CriarContexto(tenant);
        var p = Prospecto.Create(tenant, "Legado", null, null);
        var o = Orcamento.Criar(tenant, p.Id, "Antigo", 10, null);
        var contrato = Contrato.CriarDeOrcamento(o, p, DateTime.Today, null, null);
        db.Prospects.Add(p); db.Budgets.Add(o); db.Contracts.Add(contrato); await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Null((await db.Contracts.SingleAsync()).RepresentanteSocioId);
        Assert.Null((await CriarServico(db, tenant).ObterPorIdAsync(contrato.Id)).Valor!.RepresentanteSocioId);
    }

    [Fact]
    public async Task RepresentanteDoClienteCorrespondente_ValidoSemDuplicarPessoaNaLista()
    {
        var tenant = Guid.NewGuid(); await using var db = CriarContexto(tenant);
        var p = Prospecto.Create(tenant, "Empresa", "11222333000144", null);
        CompletarEnderecoProspecto(db, p);
        var o = Orcamento.Criar(tenant, p.Id, "Aceito", 10, null);
        AdicionarItensComerciais(o);
        o.Aceitar();
        db.Prospects.Add(p); db.Budgets.Add(o);
        AdicionarContratadaCompleta(db, tenant);
        var original = SocioContratualFixture.Adicionar(db, p); await db.SaveChangesAsync();
        var cliente = p.ConverterParaCliente(Guid.NewGuid()); db.Clients.Add(cliente); await db.SaveChangesAsync();
        var socioCliente = Assert.Single(cliente.Socios);
        var servico = CriarServico(db, tenant);
        Assert.Single((await servico.ObterRepresentantesAsync(o.Id)).Valor!.Socios);
        var resultado = await servico.ConverterOrcamentoAsync(new(
            o.Id,
            new DateTime(2026, 10, 1),
            2,
            new DateOnly(2027, 1, 1),
            false,
            null,
            socioCliente.Id));
        Assert.True(resultado.Sucesso, resultado.Erro);
        var contratoCriado = await db.Contracts.SingleAsync();
        Assert.Equal(socioCliente.Id, contratoCriado.RepresentanteSocioId);
        Assert.Equal(new DateTime(2028, 10, 1), contratoCriado.DataFim);
        Assert.Equal(2, contratoCriado.DuracaoAnos);
        Assert.Equal(new DateOnly(2027, 1, 1), contratoCriado.CompetenciaInicialHonorarios);
        Assert.Equal(original.PessoaId, socioCliente.PessoaId);
    }
    [Theory]
    [InlineData("sem-socios")]
    [InlineData("sem-selecao")]
    [InlineData("incompleto")]
    [InlineData("outro-prospecto")]
    [InlineData("outro-tenant")]
    public async Task Conversao_ExigeRepresentanteCompletoDoMesmoCadastroETenant(string caso)
    {
        var tenant = Guid.NewGuid();
        await using var db = CriarContexto(tenant);
        var p = Prospecto.Create(tenant, "Empresa", "11222333000144", null);
        var o = Orcamento.Criar(tenant, p.Id, "Aceito", 10, null); o.Aceitar();
        db.Prospects.Add(p); db.Budgets.Add(o);
        Socio? s = null;
        if (caso != "sem-socios") s = SocioContratualFixture.Adicionar(db, p);
        if (caso == "incompleto") s!.Pessoa.AtualizarDadosSocietarios(s.Nome, s.Cpf, null, s.Pessoa.Endereco);
        if (caso is "outro-prospecto" or "outro-tenant")
        {
            var outro = Prospecto.Create(caso == "outro-tenant" ? Guid.NewGuid() : tenant, "Outra", "99888777000166", null);
            db.Prospects.Add(outro);
            s = SocioContratualFixture.Adicionar(db, outro);
        }
        await db.SaveChangesAsync();
        var resultado = await CriarServico(db, tenant).ConverterOrcamentoAsync(new(o.Id, DateTime.Today, 4, null, false, null,
            caso == "sem-selecao" ? null : s?.Id));
        Assert.False(resultado.Sucesso);
        Assert.Empty(db.Contracts);
        Assert.Empty(db.Empresas);
        Assert.NotEmpty(resultado.ErrosValidacao);
        Assert.Contains(resultado.ErrosValidacao, erro => erro.Contains("representante", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ListaRepresentantes_AtualizaCompletudeSemEscolhaAutomatica()
    {
        var tenant = Guid.NewGuid(); await using var db = CriarContexto(tenant);
        var p = Prospecto.Create(tenant, "Empresa", "11222333000144", null);
        var o = Orcamento.Criar(tenant, p.Id, "Aceito", 10, null); o.Aceitar();
        db.Prospects.Add(p); db.Budgets.Add(o);
        var s = SocioContratualFixture.Adicionar(db, p, "SOCIO_ADMINISTRADOR");
        s.Pessoa.AtualizarDadosSocietarios(s.Nome, s.Cpf, null, s.Pessoa.Endereco);
        await db.SaveChangesAsync(); var servico = CriarServico(db, tenant);
        var antes = Assert.Single((await servico.ObterRepresentantesAsync(o.Id)).Valor!.Socios);
        Assert.False(antes.CadastroCompleto); Assert.Contains("Profissão", antes.InformacoesFaltantes);
        s.Pessoa.AtualizarDadosSocietarios(s.Nome, s.Cpf, "Arquiteta", s.Pessoa.Endereco);
        await db.SaveChangesAsync();
        var depois = Assert.Single((await servico.ObterRepresentantesAsync(o.Id)).Valor!.Socios);
        Assert.True(depois.CadastroCompleto); Assert.Equal(s.Id, depois.Id);
        Assert.Empty(db.Contracts);
    }

    [Theory]
    [InlineData("SOCIO")]
    [InlineData("SOCIO_ADMINISTRADOR")]
    public async Task ConverterOrcamentoAsync_ComOrcamentoAceito_CriaContrato(string codigo)
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        CompletarEnderecoProspecto(contexto, prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 500m, 1,
            NaturezaCobrancaItemOrcamento.HonorarioMensal));
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Processamento de folha",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 40, 12m, 2,
            NaturezaCobrancaItemOrcamento.PorFolha));
        orcamento.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        AdicionarContratadaCompleta(contexto, tenantId);
        var representante = SocioContratualFixture.Adicionar(contexto, prospecto, codigo);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            orcamento.Id,
            new DateTime(2026, 9, 1),
            4,
            new DateOnly(2026, 9, 1),
            true,
            null, RepresentanteSocioId: representante.Id));

        Assert.True(resultado.Sucesso);
        var contrato = await contexto.Contracts.SingleAsync();
        Assert.Equal(prospecto.Id, contrato.ProspectId);
        Assert.NotEqual(Guid.Empty, contrato.EmpresaId);
        Assert.Equal(contrato.EmpresaId, prospecto.EmpresaId);
        Assert.Equal(contrato.EmpresaId, Assert.Single(contexto.Empresas).Id);
        Assert.Null(contrato.ClientId);
        Assert.Equal(orcamento.Id, contrato.BudgetId);
        Assert.Equal(representante.Id, contrato.RepresentanteSocioId);
        Assert.Equal(new DateTime(2030, 9, 1), contrato.DataFim);
        Assert.Equal(4, contrato.DuracaoAnos);
        Assert.Equal(new DateOnly(2026, 9, 1), contrato.CompetenciaInicialHonorarios);

        var dadosContrato = (await servico.ObterPorIdAsync(contrato.Id)).Valor!;
        Assert.Equal(10, dadosContrato.DiaVencimento);
        Assert.Equal(500m, dadosContrato.ValorHonorarioMensal);
        Assert.Equal(12m, dadosContrato.ValorUnitarioPorFolha);
    }

    [Fact]
    public async Task ConverterOrcamentoAsync_ComContratoAtivoExigeConfirmacaoESoSubstituiAposAssinatura()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        CompletarEnderecoProspecto(contexto, prospecto);
        var empresa = Empresa.CriarDoProspecto(prospecto);
        empresa.VincularEndereco(CriarEnderecoCompleto(contexto, "Rua da Contratante").Id);
        prospecto.VincularEmpresa(empresa.Id);
        var orcamentoAtual = Orcamento.Criar(tenantId, prospecto.Id, "Contrato atual", 10, null);
        var novoOrcamento = Orcamento.Criar(tenantId, prospecto.Id, "Novo contrato", 10, null);
        AdicionarItensComerciais(novoOrcamento);
        orcamentoAtual.Aceitar();
        novoOrcamento.Aceitar();
        var contratoAtual = Contrato.CriarDeOrcamento(
            orcamentoAtual, prospecto, new DateTime(2026, 1, 1), null, null);
        contratoAtual.MarkAsSigned(new DateTime(2026, 1, 2));

        contexto.Empresas.Add(empresa);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(orcamentoAtual, novoOrcamento);
        contexto.Contracts.Add(contratoAtual);
        AdicionarContratadaCompleta(contexto, tenantId);
        var representante = SocioContratualFixture.Adicionar(contexto, prospecto);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);

        var ativos = await servico.ListarAtivosParaOrcamentoAsync(novoOrcamento.Id);
        var semConfirmacao = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            novoOrcamento.Id,
            new DateTime(2026, 10, 16),
            4,
            new DateOnly(2026, 10, 1),
            false,
            null, RepresentanteSocioId: representante.Id));

        Assert.True(ativos.Sucesso);
        Assert.Equal(contratoAtual.Id, Assert.Single(ativos.Valor!).Id);
        Assert.False(semConfirmacao.Sucesso);
        Assert.Equal(ContratoErros.SubstituicaoRequerConfirmacao, semConfirmacao.Erro);
        Assert.Equal(StatusContrato.Ativo, contratoAtual.Status);
        Assert.Single(contexto.Contracts);

        var comConfirmacao = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            novoOrcamento.Id,
            new DateTime(2026, 10, 16),
            4,
            new DateOnly(2026, 10, 1),
            true,
            null, RepresentanteSocioId: representante.Id));

        Assert.True(comConfirmacao.Sucesso, comConfirmacao.Erro);
        Assert.Equal(StatusContrato.Ativo, contratoAtual.Status);
        var contratoNovo = await contexto.Contracts.SingleAsync(contrato => contrato.Id == comConfirmacao.Valor);
        Assert.Equal(StatusContrato.Rascunho, contratoNovo.Status);
    }

    [Fact]
    public async Task EncerrarContratosExpiradosAsync_EncerraApenasVigenciasAnterioresADataDeReferencia()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var expirado = CriarContratoComVigencia(tenantId, new DateTime(2026, 10, 15));
        var terminaHoje = CriarContratoComVigencia(tenantId, new DateTime(2026, 10, 16));
        var indeterminado = CriarContratoComVigencia(tenantId, null);
        contexto.Prospects.AddRange(expirado.Prospecto, terminaHoje.Prospecto, indeterminado.Prospecto);
        contexto.Budgets.AddRange(expirado.Orcamento, terminaHoje.Orcamento, indeterminado.Orcamento);
        contexto.Contracts.AddRange(expirado.Contrato, terminaHoje.Contrato, indeterminado.Contrato);
        await contexto.SaveChangesAsync();

        var resultado = await CriarServico(contexto, tenantId)
            .EncerrarContratosExpiradosAsync(new DateOnly(2026, 10, 16));

        Assert.True(resultado.Sucesso);
        Assert.Equal(1, resultado.Valor);
        Assert.Equal(StatusContrato.Encerrado, expirado.Contrato.Status);
        Assert.Equal(StatusContrato.Ativo, terminaHoje.Contrato.Status);
        Assert.Equal(StatusContrato.Ativo, indeterminado.Contrato.Status);
    }

    private static (Prospecto Prospecto, Orcamento Orcamento, Contrato Contrato) CriarContratoComVigencia(
        Guid tenantId,
        DateTime? dataFim)
    {
        var prospecto = Prospecto.Create(tenantId, $"Empresa {Guid.NewGuid():N}", Guid.NewGuid().ToString("N")[..14], null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        var contrato = Contrato.CriarDeOrcamento(orcamento, prospecto, new DateTime(2026, 1, 1), dataFim, null);
        contrato.MarkAsSigned(new DateTime(2026, 1, 2));
        return (prospecto, orcamento, contrato);
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
            4,
            null,
            true,
            null));

        Assert.False(resultado.Sucesso);
        Assert.Contains(resultado.ErrosValidacao, erro => erro.Contains("aceito", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ConverterOrcamentoAsync_ComOrcamentoInexistente_FalhaSemCriarContrato()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);

        var resultado = await CriarServico(contexto, tenantId).ConverterOrcamentoAsync(
            new ConverterOrcamentoContratoRequisicao(
                Guid.NewGuid(), new DateTime(2026, 9, 1), 4, new DateOnly(2026, 9, 1), true, null, null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(contexto.Contracts);
    }

    [Theory]
    [InlineData("crc-contratada")]
    [InlineData("endereco-contratada")]
    [InlineData("responsavel-ausente")]
    [InlineData("endereco-responsavel")]
    [InlineData("endereco-contratante")]
    [InlineData("sem-honorario")]
    [InlineData("sem-folha")]
    [InlineData("vencimento-invalido")]
    [InlineData("orcamento-nao-aceito")]
    public async Task Conversao_ComDadosObrigatoriosPendentes_RetornaPendenciasSemCriarContrato(string caso)
    {
        var cenario = await CriarCenarioProntoAsync(
            incluirHonorarioMensal: caso != "sem-honorario",
            incluirPorFolha: caso != "sem-folha",
            diaVencimento: caso == "vencimento-invalido" ? 32 : 10,
            enderecoProspectoCompleto: caso != "endereco-contratante",
            crcContratadaInformado: caso != "crc-contratada",
            enderecoResponsavelCompleto: caso != "endereco-responsavel",
            incluirResponsavel: caso != "responsavel-ausente",
            enderecoContratadaCompleto: caso != "endereco-contratada",
            aceitarOrcamento: caso != "orcamento-nao-aceito");
        await using var contexto = cenario.Contexto;
        var servico = CriarServico(contexto, cenario.TenantId);

        var resultado = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            cenario.Orcamento.Id,
            new DateTime(2026, 9, 1),
            4,
            new DateOnly(2026, 9, 1),
            true,
            null,
            cenario.Representante.Id));

        Assert.False(resultado.Sucesso);
        Assert.Contains(ContratoErros.DadosObrigatoriosPendentes, resultado.Erro);
        Assert.NotEmpty(resultado.ErrosValidacao);
        Assert.Empty(contexto.Contracts);
        Assert.Empty(contexto.Empresas);

        var termosEsperados = caso switch
        {
            "crc-contratada" => new[] { "CRC da CONTRATADA" },
            "endereco-contratada" => new[] { "Endereço da CONTRATADA" },
            "responsavel-ausente" => new[] { "Responsável técnico" },
            "endereco-responsavel" => new[] { "Endereço do responsável técnico" },
            "endereco-contratante" => new[] { "Endereço da CONTRATANTE" },
            "sem-honorario" => new[] { "Honorário mensal" },
            "sem-folha" => new[] { "preço por folha" },
            "vencimento-invalido" => new[] { "vencimento" },
            _ => new[] { "aceito" }
        };
        Assert.All(termosEsperados, termo =>
            Assert.Contains(resultado.ErrosValidacao, pendencia => pendencia.Contains(termo, StringComparison.OrdinalIgnoreCase)));
    }

    [Theory]
    [InlineData("data-inicio")]
    [InlineData("duracao")]
    [InlineData("competencia-ausente")]
    [InlineData("competencia-invalida")]
    public async Task Conversao_ComVigenciaOuCompetenciaInvalida_RetornaPendencias(string caso)
    {
        var cenario = await CriarCenarioProntoAsync();
        await using var contexto = cenario.Contexto;
        var requisicao = new ConverterOrcamentoContratoRequisicao(
            cenario.Orcamento.Id,
            caso == "data-inicio" ? default : new DateTime(2026, 9, 1),
            caso == "duracao" ? 0 : 4,
            caso switch
            {
                "competencia-ausente" => null,
                "competencia-invalida" => new DateOnly(2026, 9, 15),
                _ => new DateOnly(2026, 9, 1)
            },
            true,
            null,
            cenario.Representante.Id);

        var resultado = await CriarServico(contexto, cenario.TenantId).ConverterOrcamentoAsync(requisicao);

        Assert.False(resultado.Sucesso);
        Assert.NotEmpty(resultado.ErrosValidacao);
        Assert.Empty(contexto.Contracts);
    }

    [Fact]
    public async Task Conversao_AgregaTodasAsPendenciasEmUmaResposta()
    {
        var cenario = await CriarCenarioProntoAsync(
            incluirHonorarioMensal: false,
            diaVencimento: 0,
            crcContratadaInformado: false,
            enderecoResponsavelCompleto: false);
        await using var contexto = cenario.Contexto;
        var requisicao = new ConverterOrcamentoContratoRequisicao(
            cenario.Orcamento.Id,
            new DateTime(2026, 9, 1),
            4,
            null,
            true,
            null,
            null);

        var resultado = await CriarServico(contexto, cenario.TenantId).ConverterOrcamentoAsync(requisicao);

        Assert.False(resultado.Sucesso);
        Assert.Contains(resultado.ErrosValidacao, pendencia => pendencia.Contains("CONTRATADA", StringComparison.Ordinal));
        Assert.Contains(resultado.ErrosValidacao, pendencia => pendencia.Contains("responsável técnico", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(resultado.ErrosValidacao, pendencia => pendencia.Contains("Honorário mensal", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(resultado.ErrosValidacao, pendencia => pendencia.Contains("vencimento", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(resultado.ErrosValidacao, pendencia => pendencia.Contains("representante", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(resultado.ErrosValidacao, pendencia => pendencia.Contains("Competência", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(contexto.Contracts);
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
    public async Task GerarPdfAsync_ComEmpresaGeradaDoProspecto_UsaDadosCadastrais()
    {
        var cenario = await CriarCenarioProntoAsync();
        await using var contexto = cenario.Contexto;
        var pdf = new ServicoPdfCapturaFake();
        var instante = new DateTimeOffset(2026, 10, 24, 1, 30, 0, TimeSpan.Zero);
        var servico = CriarServico(contexto, cenario.TenantId, pdf, new ServicoAssinaturaFake(),
            timeProvider: new RelogioContratoFixo(instante));
        var conversao = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            cenario.Orcamento.Id, new DateTime(2026, 9, 1), 4, new DateOnly(2026, 10, 1),
            true, "Condição adicional de teste.", RepresentanteSocioId: cenario.Representante.Id));
        Assert.True(conversao.Sucesso);
        contexto.ChangeTracker.Clear();

        var resultado = await servico.GerarPdfAsync(conversao.Valor);

        Assert.True(resultado.Sucesso);
        var dados = Assert.IsType<DadosPdfContrato>(pdf.UltimosDados);
        Assert.Equal("Empresa Teste", dados.Contratante.RazaoSocial);
        Assert.Equal("11222333000144", dados.Contratante.Cnpj);
        Assert.Equal("Rua da CONTRATANTE", dados.Contratante.Endereco.Logradouro);
        Assert.Equal("Ana Representante", dados.Contratante.Representante.Nome);
        Assert.Equal("Arquiteta", dados.Contratante.Representante.Profissao);
        Assert.Equal("Rua do Representante", dados.Contratante.Representante.Endereco.Logradouro);
        Assert.Equal("CRC-SC-12345", dados.Contratada.Crc);
        Assert.Equal("Rua do Responsável Técnico", dados.Contratada.ResponsavelTecnico.Endereco.Logradouro);
        Assert.Equal(500m, dados.ValorHonorarioMensal);
        Assert.Equal(12m, dados.ValorUnitarioPorFolha);
        Assert.Equal(10, dados.DiaVencimento);
        Assert.Equal(new DateOnly(2026, 9, 1), dados.DataInicio);
        Assert.Equal(new DateOnly(2030, 9, 1), dados.DataFim);
        Assert.Equal(new DateOnly(2026, 10, 1), dados.CompetenciaInicialHonorarios);
        Assert.Equal(new DateTime(2026, 10, 23, 22, 30, 0), dados.DataEmissao);
        Assert.Equal("Condição adicional de teste.", dados.ClausulasExtras);
        Assert.NotNull((await contexto.Contracts.SingleAsync()).PdfPath);
    }

    [Fact]
    public async Task GerarPdfAsync_ComEmpresaVinculada_UsaDadosDaEmpresaComoFontePrioritaria()
    {
        var cenario = await CriarCenarioProntoAsync();
        await using var contexto = cenario.Contexto;
        var empresa = Empresa.CriarConstituida(cenario.TenantId, "11222333000144",
            "Dados cadastrais da empresa", "Empresa Cadastrada", email: "empresa@teste.local",
            regimeTributario: RegimeTributario.LucroPresumido.ToString());
        empresa.VincularEndereco(CriarEnderecoCompleto(contexto, "Rua da Empresa Prioritária").Id);
        cenario.Prospecto.VincularEmpresa(empresa.Id);
        contexto.Empresas.Add(empresa);
        await contexto.SaveChangesAsync();
        var pdf = new ServicoPdfCapturaFake();
        var servico = CriarServico(contexto, cenario.TenantId, pdf);
        var conversao = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            cenario.Orcamento.Id, new DateTime(2026, 9, 1), 4, new DateOnly(2026, 9, 1),
            true, null, RepresentanteSocioId: cenario.Representante.Id));
        Assert.True(conversao.Sucesso);
        contexto.ChangeTracker.Clear();

        var resultado = await servico.GerarPdfAsync(conversao.Valor);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Dados cadastrais da empresa", pdf.UltimosDados!.Contratante.RazaoSocial);
        Assert.Equal("Empresa Cadastrada", pdf.UltimosDados.Contratante.NomeFantasia);
        Assert.Equal("LucroPresumido", pdf.UltimosDados.Contratante.RegimeTributario);
        Assert.Equal("empresa@teste.local", pdf.UltimosDados.Contratante.Email);
        Assert.Equal("Rua da Empresa Prioritária", pdf.UltimosDados.Contratante.Endereco.Logradouro);
    }

    [Fact]
    public async Task EnviarParaAssinaturaAsync_ComAutentiqueCriaSolicitacaoAssinatura()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(
            tenantId,
            "Empresa Prospect",
            "11222333000144",
            RegimeTributario.SimplesNacional);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.Aceitar();
        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 9, 1),
            null,
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
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, new DateTime(2026, 9, 1), null, null);
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
        var publicador = new PublicadorAtualizacaoTempoRealFake();
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            new ServicoAssinaturaFake(),
            publicadorAtualizacaoTempoReal: publicador);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
        Assert.NotNull(contrato.AssinadoEm);
        Assert.Single(contexto.AssinaturaEventos);
        Assert.Equal(
            new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Contrato, contrato.Id),
            Assert.Single(publicador.Atualizacoes));
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_ContratoComDataAssinaturaAnteriorVoltaParaAtivo()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var assinaturaAnterior = new DateTime(2026, 9, 26, 18, 32, 13, DateTimeKind.Utc);
        contrato.MarkAsSigned(assinaturaAnterior);
        contrato.MarcarAguardandoAssinatura();
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-10-02T19:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
        Assert.Equal(assinaturaAnterior, contrato.AssinadoEm);
        Assert.Empty(contrato.EventosDominio);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_DocumentFinishedConverteProspectoEVinculaArtefatosAoCliente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(
            tenantId,
            "Empresa Prospect",
            "11222333000144",
            RegimeTributario.SimplesNacional);
        prospecto.Socios.Add(Socio.Create(
            tenantId,
            "Socio Prospect",
            "12345678901",
            100m,
            prospectId: prospecto.Id));
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 9, 1),
            null,
            null);
        var status = new StatusAssinatura("aguardando_assinatura", "Aguardando assinatura", 10);
        var solicitacao = AssinaturaSolicitacao.Criar(tenantId, contrato.Id, status.Id, "autentique");
        solicitacao.DefinirIdentificadorExterno("doc-autentique-1");
        contexto.Prospects.Add(prospecto);
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
        var reprocessamento = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.True(reprocessamento.Sucesso, reprocessamento.Erro);
        var cliente = await contexto.Clients.Include(item => item.Socios).SingleAsync();
        Assert.Equal(prospecto.Id, cliente.ProspectOrigemId);
        Assert.Equal(cliente.Id, prospecto.ClientId);
        Assert.Equal(cliente.Id, contrato.ClientId);
        Assert.Equal(StatusProspecto.Convertido, prospecto.Status);
        Assert.Equal("Socio Prospect", Assert.Single(cliente.Socios).Nome);
        var empresa = Assert.Single(contexto.Empresas);
        Assert.Equal("SimplesNacional", empresa.RegimeTributario);
        var statusCliente = Assert.Single(contexto.StatusClientes);
        Assert.Equal("ATIVO", statusCliente.Codigo);
        Assert.Equal(statusCliente.Id, cliente.StatusClienteId);
        Assert.Equal(cliente.Id, Assert.Single(contexto.Documents).ClientId);
        Assert.Single(contexto.AssinaturaEventos);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_ProspectoJaConvertido_ReutilizaClienteExistente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Prospect", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 9, 1),
            null,
            null);
        var cliente = prospecto.ConverterParaCliente();
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
        Assert.Equal(cliente.Id, contrato.ClientId);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
        Assert.Single(contexto.Clients);
        Assert.Equal(cliente.Id, Assert.Single(contexto.Documents).ClientId);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_AoAssinarNovoContratoEncerraContratoAtivoAnterior()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (prospecto, contratoNovo, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var empresa = Empresa.CriarDoProspecto(prospecto);
        prospecto.VincularEmpresa(empresa.Id);
        contratoNovo.VincularEmpresa(empresa.Id);

        var orcamentoAnterior = Orcamento.Criar(tenantId, prospecto.Id, "Contrato anterior", 10, null);
        var contratoAnterior = Contrato.CriarDeOrcamento(
            orcamentoAnterior, prospecto, new DateTime(2025, 1, 1), null, null);
        contratoAnterior.MarkAsSigned(new DateTime(2025, 1, 2));
        contexto.Empresas.Add(empresa);
        contexto.Budgets.Add(orcamentoAnterior);
        contexto.Contracts.Add(contratoAnterior);
        await contexto.SaveChangesAsync();

        var resultado = await CriarServico(contexto, tenantId).ProcessarWebhookAutentiqueAsync(
            """{"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-10-16T10:00:00Z"}""");

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(StatusContrato.Encerrado, contratoAnterior.Status);
        Assert.Equal(StatusContrato.Ativo, contratoNovo.Status);
    }

    [Fact]
    public async Task ProcessarWebhookAutentiqueAsync_DocumentFinishedBaixaEArmazenaPdfAssinado()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        var armazenamento = new ServicoArmazenamentoFake();
        var publicador = new PublicadorAtualizacaoTempoRealFake();
        var assinatura = new ServicoAssinaturaFake(
            documentoAssinado: ResultadoDocumentoAssinado.CriarSucesso(
                new MemoryStream([10, 20, 30]),
                "contrato-final.pdf"));
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            assinatura,
            armazenamento,
            publicadorAtualizacaoTempoReal: publicador);
        var payload = """
            {"event":"document.finished","document":{"id":"doc-autentique-1"},"created_at":"2026-08-30T10:00:00Z"}
            """;

        var resultado = await servico.ProcessarWebhookAutentiqueAsync(payload);

        Assert.True(resultado.Sucesso, resultado.Erro);
        var envio = Assert.Single(armazenamento.Envios);
        Assert.Equal("contracts", envio.Container);
        Assert.Equal([10, 20, 30], envio.Conteudo);
        Assert.Contains($"contratos/{contrato.Id}/assinaturas/contrato-final.pdf", envio.FileName);
        contexto.ChangeTracker.Clear();
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
    public async Task ProcessarWebhookAutentiqueAsync_DocumentUpdatedComDadosDeAssinatura_NaoAtivaContrato()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (_, contrato, _) = await CriarContratoComSolicitacaoAsync(contexto, tenantId);
        contrato.MarcarAguardandoAssinatura();
        await contexto.SaveChangesAsync();
        var armazenamento = new ServicoArmazenamentoFake();
        var assinatura = new ServicoAssinaturaFake(
            documentoAssinado: ResultadoDocumentoAssinado.CriarSucesso(
                new MemoryStream([10, 20, 30]),
                "contrato-final.pdf"));
        var publicador = new PublicadorAtualizacaoTempoRealFake();
        var servico = CriarServico(
            contexto,
            tenantId,
            new ServicoPdfFake(),
            assinatura,
            armazenamento,
            publicadorAtualizacaoTempoReal: publicador);
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
        Assert.Equal(StatusContrato.AguardandoAssinatura, contrato.Status);
        Assert.Null(contrato.AssinadoEm);
        Assert.Empty(contexto.Documents);
        Assert.Empty(armazenamento.Envios);
        Assert.Empty(publicador.Atualizacoes);
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
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, new DateTime(2026, 9, 1), null, null);
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
        ServicoEmpresaAtual? empresaAtual = null,
        IPublicadorAtualizacaoTempoReal? publicadorAtualizacaoTempoReal = null,
        TimeProvider? timeProvider = null)
    {
        var tenantAtual = empresaAtual ?? CriarEmpresaAtual(tenantId);
        return new ContratoServico(
            new ContratoRepositorio(contexto),
            new ContabilidadeRepositorio(contexto, tenantAtual),
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto),
            new ServicoEnderecoComercialFake(),
            servicoPdf,
            servicoAssinatura,
            servicoArmazenamento ?? new ServicoArmazenamentoFake(),
            tenantAtual,
            publicadorAtualizacaoTempoReal ?? new PublicadorAtualizacaoTempoRealFake(),
            NullLogger<ContratoServico>.Instance,
            timeProvider ?? TimeProvider.System);
    }

    private static (Parceiro Parceiro, ResponsavelTecnico Responsavel) AdicionarContratadaCompleta(
        ContextoAplicacao contexto,
        Guid tenantId,
        bool crcContratadaInformado = true,
        bool enderecoResponsavelCompleto = true)
    {
        var parceiro = Parceiro.CriarDesenvolvimento(tenantId, "12345678000195", "Officio Contabilidade");
        parceiro.AtualizarDadosContabilidade(
            "12345678000195", "Officio Contabilidade", null, null, null,
            crcContratadaInformado ? "CRC-SC-12345" : null);
        parceiro.VincularEndereco(CriarEnderecoCompleto(contexto, "Rua da Contabilidade").Id);

        var responsavel = ResponsavelTecnico.Criar(
            tenantId, "Responsável Técnico", "12345678901", "Contador", "CRC-SC-54321");
        if (enderecoResponsavelCompleto)
            responsavel.VincularEndereco(CriarEnderecoCompleto(contexto, "Rua do Responsável Técnico").Id);

        contexto.Parceiros.Add(parceiro);
        contexto.ResponsaveisTecnicos.Add(responsavel);
        return (parceiro, responsavel);
    }

    private static async Task<CenarioProntidaoContrato> CriarCenarioProntoAsync(
        bool incluirHonorarioMensal = true,
        bool incluirPorFolha = true,
        int diaVencimento = 10,
        bool enderecoProspectoCompleto = true,
        bool crcContratadaInformado = true,
        bool enderecoResponsavelCompleto = true,
        bool incluirResponsavel = true,
        bool enderecoContratadaCompleto = true,
        bool aceitarOrcamento = true)
    {
        var tenantId = Guid.NewGuid();
        var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        if (enderecoProspectoCompleto)
            CompletarEnderecoProspecto(contexto, prospecto);

        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", diaVencimento, null);
        if (incluirHonorarioMensal)
            AdicionarItemComercial(orcamento, NaturezaCobrancaItemOrcamento.HonorarioMensal);
        if (incluirPorFolha)
            AdicionarItemComercial(orcamento, NaturezaCobrancaItemOrcamento.PorFolha);
        if (aceitarOrcamento)
            orcamento.Aceitar();

        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        var (contratada, responsavel) = AdicionarContratadaCompleta(
            contexto, tenantId, crcContratadaInformado, enderecoResponsavelCompleto);
        if (!incluirResponsavel && responsavel is not null)
        {
            contexto.ResponsaveisTecnicos.Remove(responsavel);
            responsavel = null;
        }
        if (!enderecoContratadaCompleto)
            contratada.VincularEndereco(null);
        var representante = SocioContratualFixture.Adicionar(contexto, prospecto);
        await contexto.SaveChangesAsync();

        return new CenarioProntidaoContrato(
            contexto, tenantId, prospecto, orcamento, contratada, responsavel, representante);
    }

    private static void CompletarEnderecoProspecto(ContextoAplicacao contexto, Prospecto prospecto)
    {
        var municipio = GarantirMunicipio(contexto);
        prospecto.VincularMunicipio(municipio.Id, "Blumenau", "SC");
        prospecto.AtualizarDadosCadastrais(
            null, null, null, null, null,
            null, null, null, null,
            "89010000", "Rua da CONTRATANTE", "10", null, "Centro", "Blumenau", "SC",
            null, null, null, null, null);
    }

    private static void AdicionarItensComerciais(Orcamento orcamento)
    {
        AdicionarItemComercial(orcamento, NaturezaCobrancaItemOrcamento.HonorarioMensal);
        AdicionarItemComercial(orcamento, NaturezaCobrancaItemOrcamento.PorFolha);
    }

    private static void AdicionarItemComercial(Orcamento orcamento, NaturezaCobrancaItemOrcamento natureza)
    {
        var porFolha = natureza == NaturezaCobrancaItemOrcamento.PorFolha;
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            orcamento.TenantId,
            orcamento.Id,
            null,
            porFolha ? "Processamento de folha" : "Honorário mensal",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            porFolha ? 40 : 1,
            porFolha ? 12m : 500m,
            porFolha ? 2 : 1,
            natureza));
    }

    private static Endereco CriarEnderecoCompleto(ContextoAplicacao contexto, string logradouro)
    {
        var municipio = GarantirMunicipio(contexto);
        var endereco = Endereco.Criar(municipio.Id, logradouro, "89010000", "10", "Centro");
        contexto.Enderecos.Add(endereco);
        return endereco;
    }

    private static Municipio GarantirMunicipio(ContextoAplicacao contexto)
    {
        var estado = contexto.Estados.Local.FirstOrDefault()
            ?? Estado.Criar("42", "SC", "Santa Catarina");
        if (contexto.Entry(estado).State == EntityState.Detached)
            contexto.Estados.Add(estado);

        var municipio = contexto.Municipios.Local.FirstOrDefault(item => item.EstadoId == estado.Id)
            ?? Municipio.Criar(estado.Id, "4202404", "Blumenau");
        if (contexto.Entry(municipio).State == EntityState.Detached)
            contexto.Municipios.Add(municipio);
        return municipio;
    }

    private sealed class PublicadorAtualizacaoTempoRealFake : IPublicadorAtualizacaoTempoReal
    {
        public List<EventoAtualizacaoTempoReal> Atualizacoes { get; } = [];

        public Task PublicarAsync(
            EventoAtualizacaoTempoReal evento,
            CancellationToken cancellationToken = default)
        {
            Atualizacoes.Add(evento);
            return Task.CompletedTask;
        }
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
        => CriarContexto(tenantId, CriarEmpresaAtual(tenantId));

    private static ContextoAplicacao CriarContexto(Guid tenantId, ServicoEmpresaAtual empresaAtual)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }

    private sealed record CenarioProntidaoContrato(
        ContextoAplicacao Contexto,
        Guid TenantId,
        Prospecto Prospecto,
        Orcamento Orcamento,
        Parceiro Contratada,
        ResponsavelTecnico? Responsavel,
        Socio Representante);

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
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, new DateTime(2026, 9, 1), null, null);
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

    private sealed class ServicoEnderecoComercialFake : IServicoEnderecoComercial
    {
        public Task<Resultado<Endereco?>> AtualizarOuCriarAsync(Endereco? endereco, EnderecoComercialRequisicao requisicao, CancellationToken cancellationToken = default)
            => CriarAsync(requisicao, cancellationToken);
        public Task<Resultado<Endereco?>> CriarAsync(
            EnderecoComercialRequisicao requisicao,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Resultado<Endereco?>.CriarSucesso(null!));

        public Task<Resultado<Endereco?>> CriarDoProspectoAsync(
            Prospecto prospecto,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Resultado<Endereco?>.CriarSucesso(null));
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

        public Task<ResultadoEstadoAssinatura> ConsultarEstadoDocumentoAsync(string documentoId, CancellationToken ct = default)
            => Task.FromResult(ResultadoEstadoAssinatura.CriarSucesso(false));

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

        public Task ExcluirAsync(string path, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed record EnvioArmazenamento(string FileName, string Container, byte[] Conteudo);
    [Fact]
    public async Task GerarPdfAsync_ComResponsavelIncompleto_NaoDeveGerarNemPersistirPdf()
    {
        var cenario = await CriarCenarioProntoAsync();
        await using var contexto = cenario.Contexto;
        var pdf = new ServicoPdfCapturaFake();
        var servico = CriarServico(contexto, cenario.TenantId, pdf);
        var conversao = await servico.ConverterOrcamentoAsync(new ConverterOrcamentoContratoRequisicao(
            cenario.Orcamento.Id, new DateTime(2026, 9, 1), 4, new DateOnly(2026, 9, 1),
            true, null, RepresentanteSocioId: cenario.Representante.Id));
        Assert.True(conversao.Sucesso);
        cenario.Responsavel!.VincularEndereco(null);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();

        var resultado = await servico.GerarPdfAsync(conversao.Valor);

        Assert.False(resultado.Sucesso);
        Assert.Null(pdf.UltimosDados);
        Assert.Null((await contexto.Contracts.SingleAsync()).PdfPath);
    }

    private sealed class RelogioContratoFixo(DateTimeOffset instante) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => instante;
    }

}
