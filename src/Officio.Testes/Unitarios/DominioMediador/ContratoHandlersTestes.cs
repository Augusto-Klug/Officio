using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;

namespace Officio.Testes.Unitarios.DominioMediador;

public class ContratoHandlersTestes
{
    [Fact]
    public async Task ConverterOrcamentoParaContrato_ChamaServicoERetornaId()
    {
        var contratoId = Guid.NewGuid();
        var servico = new ContratoServicoFake(ResultadoOperacao<Guid>.CriarSucesso(contratoId));
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
        var contrato = CriarContratoResultado();
        var handler = new ListarContratosHandler(
            new ContratoServicoFake(ResultadoOperacao<List<ContratoResultado>>.CriarSucesso([contrato])));

        var resultado = await handler.Handle(new ListarContratosConsulta(), CancellationToken.None);

        var dto = Assert.Single(resultado.Valor!);
        Assert.Equal(contrato.Id, dto.Id);
        Assert.Equal(contrato.RazaoSocial, dto.RazaoSocial);
        Assert.Equal(contrato.PdfGerado, dto.PdfGerado);
    }

    [Fact]
    public async Task ObterContratoPorId_MapeiaResultadoParaDto()
    {
        var contrato = CriarContratoResultado();
        var handler = new ObterContratoPorIdHandler(
            new ContratoServicoFake(ResultadoOperacao<ContratoResultado>.CriarSucesso(contrato)));

        var resultado = await handler.Handle(
            new ObterContratoPorIdConsulta(contrato.Id), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(contrato.Id, resultado.Valor!.Id);
        Assert.Equal(contrato.ValorTotal, resultado.Valor.ValorTotal);
    }

    private static ContratoResultado CriarContratoResultado()
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
        private readonly ResultadoOperacao<Guid>? conversao;
        private readonly ResultadoOperacao<List<ContratoResultado>>? listagem;
        private readonly ResultadoOperacao<ContratoResultado>? detalhe;

        public ContratoServicoFake(ResultadoOperacao<Guid> resultado) => conversao = resultado;
        public ContratoServicoFake(ResultadoOperacao<List<ContratoResultado>> resultado) => listagem = resultado;
        public ContratoServicoFake(ResultadoOperacao<ContratoResultado> resultado) => detalhe = resultado;

        public Task<ResultadoOperacao<Guid>> ConverterOrcamentoAsync(
            ConverterOrcamentoContratoRequisicao requisicao, CancellationToken ct = default)
            => Task.FromResult(conversao ?? throw new NotImplementedException());

        public Task<ResultadoOperacao<List<ContratoResultado>>> ListarAsync(CancellationToken ct = default)
            => Task.FromResult(listagem ?? throw new NotImplementedException());

        public Task<ResultadoOperacao<ContratoResultado>> ObterPorIdAsync(
            Guid contratoId, CancellationToken ct = default)
            => Task.FromResult(detalhe ?? throw new NotImplementedException());

        public Task<ResultadoOperacao<PdfContratoResultado>> GerarPdfAsync(
            Guid contratoId, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<ResultadoOperacao<EnvioAssinaturaResultado>> EnviarParaAssinaturaAsync(
            EnviarAssinaturaRequisicao requisicao, CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}
