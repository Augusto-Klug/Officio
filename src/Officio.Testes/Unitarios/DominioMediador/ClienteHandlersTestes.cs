using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.AtualizarCliente;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.VincularCodigoSci;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ListarClientes;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ObterClientePorId;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioMediador;

public class ClienteHandlersTestes
{
    [Fact]
    public async Task ListarClientes_FiltraPorTenant()
    {
        var tenantId = Guid.NewGuid();
        var tenantEstrangeiroId = Guid.NewGuid();
        var nomeBanco = Guid.NewGuid().ToString();

        await using (var contextoEstrangeiro = CriarContexto(tenantEstrangeiroId, nomeBanco))
        {
            contextoEstrangeiro.Clients.Add(CriarCliente(tenantEstrangeiroId, "Cliente Estrangeiro"));
            await contextoEstrangeiro.SaveChangesAsync();
        }

        await using var contexto = CriarContexto(tenantId, nomeBanco);
        var cliente = CriarCliente(tenantId, "Cliente Local");
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();
        var handler = new ListarClientesHandler(new ClienteServico(new ClienteRepositorio(contexto)));

        var resultado = await handler.Handle(new ListarClientesConsulta(null, null), CancellationToken.None);

        var dto = Assert.Single(resultado.Valor!);
        Assert.Equal(cliente.Id, dto.Id);
        Assert.Equal("Cliente Local", dto.RazaoSocial);
    }

    [Fact]
    public async Task ObterClientePorId_RetornaDetalheComRelacionamentos()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = CriarCliente(tenantId, "Cliente Completo");
        var socio = Socio.Create(tenantId, "Maria Socia", "12345678900", 50m, clientId: cliente.Id);
        var contrato = Contrato.CreateFromBudget(
            Orcamento.Criar(tenantId, Guid.NewGuid(), "Orcamento", 10, null),
            cliente.Id,
            DateTime.UtcNow.Date,
            null,
            true,
            null);
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.9m, clientId: cliente.Id);
        contexto.Clients.Add(cliente);
        contexto.Socios.Add(socio);
        contexto.Contracts.Add(contrato);
        contexto.Documents.Add(documento);
        await contexto.SaveChangesAsync();
        var handler = new ObterClientePorIdHandler(new ClienteServico(new ClienteRepositorio(contexto)));

        var resultado = await handler.Handle(new ObterClientePorIdConsulta(cliente.Id), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(cliente.Id, resultado.Valor!.Id);
        Assert.Single(resultado.Valor.Socios);
        Assert.Single(resultado.Valor.Contratos);
        Assert.Single(resultado.Valor.Documentos);
    }

    [Fact]
    public async Task AtualizarCliente_AtualizaDadosPrincipais()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = CriarCliente(tenantId, "Cliente Antigo");
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();
        var handler = new AtualizarClienteHandler(new ClienteServico(new ClienteRepositorio(contexto)));

        var resultado = await handler.Handle(new AtualizarClienteComando(
            cliente.Id,
            "Cliente Novo",
            "Fantasia",
            "11222333000144",
            null,
            RegimeTributario.SimplesNacional,
            "contato@cliente.test",
            "4733334444",
            "47999998888",
            "89000000",
            "Rua A",
            "123",
            "Centro",
            "Blumenau",
            "sc",
            "Observacao"), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Cliente Novo", cliente.RazaoSocial);
        Assert.Equal("SC", cliente.Uf);
        Assert.Equal(RegimeTributario.SimplesNacional, cliente.RegimeTributario);
    }

    [Fact]
    public async Task VincularCodigoSci_QuandoClienteExiste_PersisteCodigo()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = CriarCliente(tenantId, "Cliente SCI");
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();
        var handler = new VincularCodigoSciHandler(new ClienteServico(new ClienteRepositorio(contexto)));

        var resultado = await handler.Handle(new VincularCodigoSciComando(cliente.Id, "SCI-123"), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal("SCI-123", cliente.SciCodigo);
    }

    private static Cliente CriarCliente(Guid tenantId, string razaoSocial)
    {
        var prospecto = Prospecto.Create(tenantId, razaoSocial, "11222333000144", null);
        return Cliente.CreateFromProspect(prospecto);
    }

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
}
