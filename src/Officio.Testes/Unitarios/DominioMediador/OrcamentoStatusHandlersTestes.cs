using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Contratos;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioMediador;

public class OrcamentoStatusHandlersTestes
{
    [Fact]
    public async Task AceitarOrcamento_QuandoRascunho_MarcaComoAceito()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(orcamento.Id),
            CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync()).Status);
    }

    [Fact]
    public async Task AceitarOrcamento_QuandoJaAceito_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        orcamento.Aceitar();
        await contexto.SaveChangesAsync();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(orcamento.Id),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("aceito", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AceitarOrcamento_ConfirmadoSubstituiAceitoAnteriorEMantemHistorico()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta anterior", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        anterior.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        await contexto.SaveChangesAsync();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(novo.Id, ConfirmarSubstituicao: true),
            CancellationToken.None);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusOrcamento.Substituido, (await contexto.Budgets.SingleAsync(item => item.Id == anterior.Id)).Status);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync(item => item.Id == novo.Id)).Status);
        Assert.Equal(2, await contexto.Budgets.CountAsync(orcamento => orcamento.ProspectId == prospecto.Id));
        Assert.Equal(1, await contexto.Budgets.CountAsync(orcamento =>
            orcamento.ProspectId == prospecto.Id && orcamento.Status == StatusOrcamento.Aceito));
    }

    [Fact]
    public async Task AceitarOrcamento_SemConfirmacaoMantemStatusDosOrcamentos()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta anterior", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        anterior.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        await contexto.SaveChangesAsync();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(new AceitarOrcamentoComando(novo.Id), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Equal(OrcamentoErros.AceiteRequerConfirmacao, resultado.Erro);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync(item => item.Id == anterior.Id)).Status);
        Assert.Equal(StatusOrcamento.Rascunho, (await contexto.Budgets.SingleAsync(item => item.Id == novo.Id)).Status);
    }

    [Fact]
    public async Task AceitarOrcamento_ContratoRascunhoAnteriorCancelaEPermiteSubstituicaoConfirmada()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta anterior", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        anterior.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        contexto.Contracts.Add(Contrato.CriarDeOrcamento(anterior, prospecto, DateTime.Today, null, null));
        await contexto.SaveChangesAsync();
        var assinatura = new ServicoAssinaturaOrcamentoFake();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId, assinatura));

        var semConfirmacao = await handler.Handle(
            new AceitarOrcamentoComando(novo.Id),
            CancellationToken.None);

        Assert.False(semConfirmacao.Sucesso);
        Assert.Equal(OrcamentoErros.AceiteRequerConfirmacao, semConfirmacao.Erro);

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(novo.Id, ConfirmarSubstituicao: true),
            CancellationToken.None);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusOrcamento.Substituido, (await contexto.Budgets.SingleAsync(item => item.Id == anterior.Id)).Status);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync(item => item.Id == novo.Id)).Status);
        Assert.Equal(StatusContrato.Cancelado, (await contexto.Contracts.SingleAsync()).Status);
        Assert.Equal(0, assinatura.DocumentosBloqueados);
    }

    [Fact]
    public async Task AceitarOrcamento_ContratoAguardandoAssinaturaBloqueiaAutentiqueAntesDeSubstituir()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta anterior", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        anterior.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        var contrato = Contrato.CriarDeOrcamento(anterior, prospecto, DateTime.Today, null, null);
        contrato.MarcarAguardandoAssinatura();
        var solicitacao = AssinaturaSolicitacao.Criar(tenantId, contrato.Id, Guid.NewGuid(), "autentique");
        solicitacao.DefinirIdentificadorExterno("documento-antigo");
        solicitacao.AdicionarSignatario("Sócio", "socio@empresa.test", "contratante", 1);
        solicitacao.AdicionarSignatario("Contador", "contador@contabilidade.test", "contratada", 2);
        contrato.SolicitacoesAssinatura.Add(solicitacao);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();

        var assinatura = new ServicoAssinaturaOrcamentoFake();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId, assinatura));
        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(novo.Id, ConfirmarSubstituicao: true),
            CancellationToken.None);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(1, assinatura.DocumentosBloqueados);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusContrato.Cancelado, (await contexto.Contracts.SingleAsync()).Status);
        Assert.Equal(StatusOrcamento.Substituido, (await contexto.Budgets.SingleAsync(item => item.Id == anterior.Id)).Status);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync(item => item.Id == novo.Id)).Status);
    }

    [Fact]
    public async Task AceitarOrcamento_ContratoAtivoAnteriorPermaneceAtivoDuranteAceiteDoNovoOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta anterior", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        anterior.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        var contrato = Contrato.CriarDeOrcamento(anterior, prospecto, DateTime.Today, null, null);
        contrato.MarkAsSigned();
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(novo.Id, ConfirmarSubstituicao: true),
            CancellationToken.None);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusContrato.Ativo, (await contexto.Contracts.SingleAsync()).Status);
        Assert.Equal(StatusOrcamento.Substituido, (await contexto.Budgets.SingleAsync(item => item.Id == anterior.Id)).Status);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync(item => item.Id == novo.Id)).Status);
    }

    [Fact]
    public async Task AceitarOrcamento_ContratoJaCanceladoNaoImpedeSubstituicaoConfirmada()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta anterior", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        anterior.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        var contrato = Contrato.CriarDeOrcamento(anterior, prospecto, DateTime.Today, null, null);
        contrato.Cancelar();
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(novo.Id, ConfirmarSubstituicao: true),
            CancellationToken.None);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusContrato.Cancelado, (await contexto.Contracts.SingleAsync()).Status);
        Assert.Equal(StatusOrcamento.Substituido, (await contexto.Budgets.SingleAsync(item => item.Id == anterior.Id)).Status);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync(item => item.Id == novo.Id)).Status);
    }

    [Fact]
    public async Task AceitarOrcamento_FalhaAoBloquearAutentiqueMantemOrcamentosEContratoInalterados()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var anterior = Orcamento.Criar(tenantId, prospecto.Id, "Proposta anterior", 10, null);
        var novo = Orcamento.Criar(tenantId, prospecto.Id, "Nova proposta", 10, null);
        anterior.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(anterior, novo);
        var contrato = Contrato.CriarDeOrcamento(anterior, prospecto, DateTime.Today, null, null);
        contrato.MarcarAguardandoAssinatura();
        var solicitacao = AssinaturaSolicitacao.Criar(tenantId, contrato.Id, Guid.NewGuid(), "autentique");
        solicitacao.DefinirIdentificadorExterno("documento-antigo");
        solicitacao.AdicionarSignatario("Socio", "socio@empresa.test", "contratante", 1);
        solicitacao.AdicionarSignatario("Contador", "contador@contabilidade.test", "contratada", 2);
        contrato.SolicitacoesAssinatura.Add(solicitacao);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();

        var assinatura = new ServicoAssinaturaOrcamentoFake { BloqueioSucesso = false };
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantId, assinatura));
        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(novo.Id, ConfirmarSubstituicao: true),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusContrato.AguardandoAssinatura, (await contexto.Contracts.SingleAsync()).Status);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync(item => item.Id == anterior.Id)).Status);
        Assert.Equal(StatusOrcamento.Rascunho, (await contexto.Budgets.SingleAsync(item => item.Id == novo.Id)).Status);
    }

    [Fact]
    public async Task AceitarOrcamento_IsolaBuscaDeAceitosPorTenant()
    {
        var tenantAtual = Guid.NewGuid();
        var outroTenant = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantAtual);
        var prospectoIdCompartilhado = Guid.NewGuid();
        var aceitoDeOutroTenant = Orcamento.Criar(outroTenant, prospectoIdCompartilhado, "Outro tenant", 10, null);
        var novo = Orcamento.Criar(tenantAtual, prospectoIdCompartilhado, "Tenant atual", 10, null);
        aceitoDeOutroTenant.Aceitar();
        contexto.Budgets.AddRange(aceitoDeOutroTenant, novo);
        await contexto.SaveChangesAsync();
        var handler = new AceitarOrcamentoHandler(CriarServico(contexto, tenantAtual));

        var resultado = await handler.Handle(new AceitarOrcamentoComando(novo.Id), CancellationToken.None);

        Assert.True(resultado.Sucesso, resultado.Erro);
        contexto.ChangeTracker.Clear();
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync()).Status);
    }

    [Fact]
    public void ModeloOrcamento_PermiteHistoricoEMantemIndiceUnicoParaAceitoPorTenantEProspecto()
    {
        var tenantId = Guid.NewGuid();
        using var contexto = CriarContexto(tenantId);
        var modelo = contexto.Model.FindEntityType(typeof(Orcamento))!;
        var indiceAceito = Assert.Single(modelo.GetIndexes(), indice => indice.IsUnique);

        Assert.Equal(new[] { "TenantId", "ProspectId", "Status" }, indiceAceito.Properties.Select(propriedade => propriedade.Name));
        Assert.Contains("'Aceito'", indiceAceito.GetFilter());
        Assert.Contains(modelo.GetIndexes(), indice =>
            !indice.IsUnique && indice.Properties.Select(propriedade => propriedade.Name).SequenceEqual(["TenantId", "ProspectId"]));
    }

    [Fact]
    public async Task RecusarOrcamento_SemMotivo_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new RecusarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new RecusarOrcamentoComando(orcamento.Id, ""),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("motivo", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecusarOrcamento_ComMotivo_PersisteEExibeMotivo()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new RecusarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new RecusarOrcamentoComando(orcamento.Id, "Preco acima do esperado"),
            CancellationToken.None);

        Assert.True(resultado.Sucesso);
        contexto.ChangeTracker.Clear();

        var orcamentoPersistido = await contexto.Budgets.SingleAsync();
        var consultaHandler = new ObterOrcamentosPorProspectoHandler(CriarServico(contexto, tenantId));
        var consulta = await consultaHandler.Handle(
            new ObterOrcamentosPorProspectoConsulta(orcamento.ProspectId),
            CancellationToken.None);

        Assert.Equal(StatusOrcamento.Recusado, orcamentoPersistido.Status);
        Assert.Equal("Preco acima do esperado", orcamentoPersistido.MotivoRecusa);
        Assert.Equal("Preco acima do esperado", Assert.Single(consulta.Valor!).MotivoRecusa);
    }

    [Fact]
    public async Task AlterarStatus_ParaEnviado_MarcaProspectoComoPropostaEnviada()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new AlterarStatusOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AlterarStatusOrcamentoComando(orcamento.Id, StatusOrcamento.Enviado),
            CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusProspecto.PropostaEnviada, (await contexto.Prospects.SingleAsync()).Status);
    }

    [Fact]
    public async Task AlterarStatus_ParaRecusado_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new AlterarStatusOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AlterarStatusOrcamentoComando(orcamento.Id, StatusOrcamento.Recusado),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Aceitar", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AlterarStatus_QuandoOrcamentoAceito_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        orcamento.Aceitar();
        await contexto.SaveChangesAsync();
        var handler = new AlterarStatusOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new AlterarStatusOrcamentoComando(orcamento.Id, StatusOrcamento.Rascunho),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("terminal", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Orcamento> CriarOrcamentoAsync(ContextoAplicacao contexto, Guid tenantId)
    {
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        return orcamento;
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ContextoAplicacao(options, CriarEmpresaAtual(tenantId), new PublicadorEventosFake());
    }

    private static IOrcamentoServico CriarServico(
        ContextoAplicacao contexto,
        Guid tenantId,
        IServicoAssinatura? assinatura = null)
        => new OrcamentoServico(
            new OrcamentoRepositorio(contexto),
            new ContratoRepositorio(contexto),
            assinatura ?? new ServicoAssinaturaOrcamentoFake(),
            new ProspectoRepositorio(contexto),
            CriarEmpresaAtual(tenantId),
            new PublicadorAtualizacaoTempoRealNulo());

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return empresaAtual;
    }
}
