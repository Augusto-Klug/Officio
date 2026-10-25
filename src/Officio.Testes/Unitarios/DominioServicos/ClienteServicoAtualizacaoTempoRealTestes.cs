using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class ClienteServicoAtualizacaoTempoRealTestes
{
    [Fact]
    public async Task AtualizarAsync_AposPersistir_PublicaAtualizacaoDoCliente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = await CriarClienteAsync(contexto, tenantId);
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, publicador);

        var resultado = await servico.AtualizarAsync(new AtualizarClienteRequisicao(
            cliente.Id, "Empresa atualizada", null, "11222333000144", null, null,
            "contato@empresa.local", "47999999999", null, null, null, null, null, null, null, null));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(cliente.Id, resultado.Valor);
        Assert.Equal(new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Cliente, cliente.Id),
            Assert.Single(publicador.Eventos));
    }

    [Fact]
    public async Task VincularCodigoSciAsync_AposPersistir_PublicaAtualizacaoDoCliente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = await CriarClienteAsync(contexto, tenantId);
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, publicador);

        var resultado = await servico.VincularCodigoSciAsync(new VincularCodigoSciRequisicao(cliente.Id, "SCI-001"));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Cliente, cliente.Id),
            Assert.Single(publicador.Eventos));
    }

    private static ClienteServico CriarServico(ContextoAplicacao contexto, IPublicadorAtualizacaoTempoReal publicador)
        => new(new ClienteRepositorio(contexto), new ServicoEnderecoFake(), publicador);

    private static async Task<Cliente> CriarClienteAsync(ContextoAplicacao contexto, Guid tenantId)
    {
        contexto.Parceiros.Add(Parceiro.CriarDesenvolvimento(tenantId, "11222333000144", "Parceiro Teste"));
        var empresa = Empresa.Create(tenantId, "Empresa original", "22333444000155");
        var status = new StatusCliente("ATIVO", "Ativo", 1);
        var cliente = Cliente.CriarParaEmpresa(tenantId, empresa.Id, statusClienteId: status.Id);
        contexto.Empresas.Add(empresa);
        contexto.StatusClientes.Add(status);
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        return await contexto.Clients.SingleAsync();
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }

    private sealed class ServicoEnderecoFake : IServicoEnderecoComercial
    {
        public Task<Resultado<Endereco?>> AtualizarOuCriarAsync(Endereco? endereco, EnderecoComercialRequisicao requisicao, CancellationToken cancellationToken = default)
            => CriarAsync(requisicao, cancellationToken);
        public Task<Resultado<Endereco?>> CriarAsync(
            EnderecoComercialRequisicao requisicao,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Resultado<Endereco?>.CriarSucesso(null));

        public Task<Resultado<Endereco?>> CriarDoProspectoAsync(
            Prospecto prospecto,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Resultado<Endereco?>.CriarSucesso(null));
    }

    private sealed class PublicadorAtualizacaoFake : IPublicadorAtualizacaoTempoReal
    {
        public List<EventoAtualizacaoTempoReal> Eventos { get; } = [];

        public Task PublicarAsync(EventoAtualizacaoTempoReal evento, CancellationToken cancellationToken = default)
        {
            Eventos.Add(evento);
            return Task.CompletedTask;
        }
    }

    private sealed class PublicadorEventosFake : MediatR.IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : MediatR.INotification
            => Task.CompletedTask;
    }
}
