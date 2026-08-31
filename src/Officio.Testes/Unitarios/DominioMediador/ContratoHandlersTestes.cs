using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;

namespace Officio.Testes.Unitarios.DominioMediador;

public class ContratoHandlersTestes
{
    [Fact]
    public async Task ConverterOrcamentoParaContrato_ChamaServicoERetornaId()
    {
        var contratoId = Guid.NewGuid();
        var servico = new ContratoServicoFake(Resultado<Guid>.CriarSucesso(contratoId));
        var handler = new ConverterOrcamentoParaContratoHandler(servico);

        var resultado = await handler.Handle(new ConverterOrcamentoParaContratoComando(
            Guid.NewGuid(),
            new DateTime(2026, 9, 1),
            null,
            true,
            "Clausula"), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(contratoId, resultado.Valor);
    }

    [Fact]
    public async Task ListarContratos_MapeiaResultadoParaDto()
    {
        var contrato = CriarContratoDto();
        var handler = new ListarContratosHandler(
            new ContratoServicoFake(Resultado<List<ContratoDto>>.CriarSucesso([contrato])));

        var resultado = await handler.Handle(new ListarContratosConsulta(), CancellationToken.None);

        var dto = Assert.Single(resultado.Valor!);
        Assert.Equal(contrato.Id, dto.Id);
        Assert.Equal(contrato.RazaoSocial, dto.RazaoSocial);
        Assert.Equal(contrato.PdfGerado, dto.PdfGerado);
    }

    [Fact]
    public async Task ObterContratoPorId_MapeiaResultadoParaDto()
    {
        var contrato = CriarContratoDto();
        var handler = new ObterContratoPorIdHandler(
            new ContratoServicoFake(Resultado<ContratoDto>.CriarSucesso(contrato)));

        var resultado = await handler.Handle(
            new ObterContratoPorIdConsulta(contrato.Id), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(contrato.Id, resultado.Valor!.Id);
        Assert.Equal(contrato.ValorTotal, resultado.Valor.ValorTotal);
    }

    [Fact]
    public async Task ProcessarWebhookAutentique_ChamaServico()
    {
        var handler = new ProcessarWebhookAutentiqueHandler(
            new ContratoServicoFake(Resultado<bool>.CriarSucesso(true)));

        var resultado = await handler.Handle(
            new ProcessarWebhookAutentiqueComando("""{"event":"document.finished"}"""),
            CancellationToken.None);

        Assert.True(resultado.Sucesso);
    }

    private static ContratoDto CriarContratoDto()
        => new(
            Guid.NewGuid(),
            "CTR-2026-0001",
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            "Empresa Teste",
            "Ativo",
            1500m,
            10,
            new DateTime(2026, 9, 1),
            null,
            true,
            "Clausula",
            false);

    private sealed class ContratoServicoFake : IContratoServico
    {
        private readonly Resultado<Guid>? conversao;
        private readonly Resultado<List<ContratoDto>>? listagem;
        private readonly Resultado<ContratoDto>? detalhe;
        private readonly Resultado<bool>? webhook;

        public ContratoServicoFake(Resultado<Guid> resultado) => conversao = resultado;
        public ContratoServicoFake(Resultado<List<ContratoDto>> resultado) => listagem = resultado;
        public ContratoServicoFake(Resultado<ContratoDto> resultado) => detalhe = resultado;
        public ContratoServicoFake(Resultado<bool> resultado) => webhook = resultado;

        public Task<Resultado<Guid>> ConverterOrcamentoAsync(
            ConverterOrcamentoContratoRequisicao requisicao, CancellationToken ct = default)
            => Task.FromResult(conversao ?? throw new NotImplementedException());

        public Task<Resultado<List<ContratoDto>>> ListarAsync(CancellationToken ct = default)
            => Task.FromResult(listagem ?? throw new NotImplementedException());

        public Task<Resultado<ContratoDto>> ObterPorIdAsync(
            Guid contratoId, CancellationToken ct = default)
            => Task.FromResult(detalhe ?? throw new NotImplementedException());

        public Task<Resultado<ResultadoPdfContratoDto>> GerarPdfAsync(
            Guid contratoId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Resultado<SendForSignatureResultDto>> EnviarParaAssinaturaAsync(
            EnviarAssinaturaRequisicao requisicao, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Resultado<bool>> ProcessarWebhookAutentiqueAsync(
            string payload, CancellationToken ct = default)
            => Task.FromResult(webhook ?? throw new NotImplementedException());
    }
}
