using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Enumeradores;
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
            cliente.Id, "Empresa atualizada", null, "22333444000155", null, null,
            "contato@empresa.local", "47999999999", null, null, null, null, null, null, null, null));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(cliente.Id, resultado.Valor);
        Assert.Equal(new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Cliente, cliente.Id),
            Assert.Single(publicador.Eventos));
    }

    [Fact]
    public async Task AtualizarAsync_AlteraSocio_CriaNovaVigenciaSemModificarHistorico()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = await CriarClienteAsync(contexto, tenantId);
        var socioAntigo = Socio.Create(tenantId, "Socio Original", "12345678900", 40m, clientId: cliente.Id);
        contexto.Socios.Add(socioAntigo);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, publicador);
        var dadosCliente = await new ClienteRepositorio(contexto).ObterPorIdAsync(cliente.Id);
        var socioId = Assert.Single(dadosCliente!.Socios).Id;

        var resultado = await servico.AtualizarAsync(new AtualizarClienteRequisicao(
            cliente.Id, dadosCliente.Empresa.RazaoSocial, null, dadosCliente.Empresa.Cnpj, null, null,
            null, null, null, null, null, null, null, null, null, null,
            Socios: [new CriarSocioRequisicao("Socio Original", "12345678900", 65m, socioId)]));

        Assert.True(resultado.Sucesso, resultado.Erro);
        var socios = await contexto.Socios.Where(s => s.ClientId == cliente.Id).Include(s => s.Pessoa).ToListAsync();
        Assert.Equal(2, socios.Count);
        var historico = socios.Single(s => s.Id == socioId);
        var atual = socios.Single(s => s.Ativo);
        Assert.False(historico.Ativo);
        Assert.NotNull(historico.VigenteAte);
        Assert.Equal(40m, historico.Participacao);
        Assert.Equal("Socio Original", historico.Pessoa.Nome);
        Assert.NotEqual(historico.PessoaId, atual.PessoaId);
        Assert.Equal(65m, atual.Participacao);
    }

    [Fact]
    public async Task AtualizarAsync_SemAlteracaoSocietaria_NaoCriaNovaVigencia()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = await CriarClienteAsync(contexto, tenantId);
        var socio = Socio.Create(tenantId, "Socio Atual", "12345678900", 40m, clientId: cliente.Id);
        contexto.Socios.Add(socio);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var servico = CriarServico(contexto, new PublicadorAtualizacaoFake());
        var carregado = await new ClienteRepositorio(contexto).ObterPorIdAsync(cliente.Id);
        var atual = Assert.Single(carregado!.Socios);

        var resultado = await servico.AtualizarAsync(new AtualizarClienteRequisicao(
            cliente.Id, carregado.Empresa.RazaoSocial, null, carregado.Empresa.Cnpj, null, null,
            null, null, null, null, null, null, null, null, null, null,
            Socios: [new CriarSocioRequisicao(atual.Nome, atual.Cpf, atual.Participacao, atual.Id)]));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Single(await contexto.Socios.Where(s => s.ClientId == cliente.Id).ToListAsync());
        Assert.True((await contexto.Socios.SingleAsync(s => s.Id == atual.Id)).Ativo);
    }

    [Fact]
    public async Task AtualizarAsync_AlteraEnderecoSocio_PreservaEnderecoNaVersaoHistorica()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var cliente = await CriarClienteAsync(contexto, tenantId);
        var estado = Estado.Criar("42", "SC", "Santa Catarina");
        var municipio = Municipio.Criar(estado.Id, "4202404", "Blumenau");
        contexto.Estados.Add(estado);
        contexto.Municipios.Add(municipio);
        var enderecoOriginal = Endereco.Criar(municipio.Id, "Rua Original", "89000000", "10", "Centro", "Casa");
        var socio = Socio.Create(tenantId, "Socio Endereco", "12345678900", 40m, clientId: cliente.Id);
        socio.AplicarDados("Socio Endereco", "12345678900", 40m, null, null, enderecoOriginal);
        contexto.Socios.Add(socio);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();

        var carregado = await new ClienteRepositorio(contexto).ObterPorIdAsync(cliente.Id);
        var socioAtual = Assert.Single(carregado!.Socios);
        var servico = CriarServico(contexto, new PublicadorAtualizacaoFake());
        var resultado = await servico.AtualizarAsync(new AtualizarClienteRequisicao(
            cliente.Id, carregado.Empresa.RazaoSocial, null, carregado.Empresa.Cnpj, null, null,
            null, null, null, null, null, null, null, null, null, null,
            Socios: [new CriarSocioRequisicao(socioAtual.Nome, socioAtual.Cpf, socioAtual.Participacao, socioAtual.Id,
                Endereco: new EnderecoSocioRequisicao("89000001", "Rua Nova", "20", "Apto 2", "Velha", "Blumenau", "SC", "4202404"))]));

        Assert.True(resultado.Sucesso, resultado.Erro);
        var versoes = await contexto.Socios.Where(s => s.ClientId == cliente.Id).Include(s => s.Pessoa).ThenInclude(p => p.Endereco).ToListAsync();
        var historico = versoes.Single(s => !s.Ativo);
        var atual = versoes.Single(s => s.Ativo);
        Assert.Equal("Rua Original", historico.Pessoa.Endereco!.Logradouro);
        Assert.Equal("89000000", historico.Pessoa.Endereco.Cep);
        Assert.Equal("Rua Nova", atual.Pessoa.Endereco!.Logradouro);
        Assert.Equal("89000001", atual.Pessoa.Endereco.Cep);
    }

    [Fact]
    public async Task AtualizarAsync_ContratoRascunhoExigeConfirmacaoEOCancelaAoConfirmar()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        contexto.Parceiros.Add(Parceiro.CriarDesenvolvimento(tenantId, "11222333000144", "Parceiro Teste"));
        var prospecto = Prospecto.Create(tenantId, "Empresa Original", "22333444000155", null);
        var empresa = Empresa.CriarDoProspecto(prospecto);
        prospecto.VincularEmpresa(empresa.Id);
        var status = new StatusCliente("ATIVO", "Ativo", 1);
        var cliente = Cliente.CreateFromProspect(prospecto, status.Id);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Orcamento", 1, null);
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, DateTime.UtcNow.Date, null, null);
        contexto.Prospects.Add(prospecto);
        contexto.Empresas.Add(empresa);
        contexto.StatusClientes.Add(status);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();

        var carregado = await new ClienteRepositorio(contexto).ObterPorIdAsync(cliente.Id);
        var servico = CriarServico(contexto, new PublicadorAtualizacaoFake());
        var semConfirmacao = await servico.AtualizarAsync(new AtualizarClienteRequisicao(
            cliente.Id, "Empresa Atualizada", null, empresa.Cnpj, null, null, null, null, null,
            null, null, null, null, null, null, null));

        Assert.False(semConfirmacao.Sucesso);
        Assert.Contains("CONFIRMACAO_CANCELAMENTO_CONTRATOS", semConfirmacao.Erro);
        Assert.Equal(Officio.Dominio.Enumeradores.StatusContrato.Rascunho,
            (await contexto.Contracts.SingleAsync(c => c.Id == contrato.Id)).Status);

        var confirmado = await servico.AtualizarAsync(new AtualizarClienteRequisicao(
            cliente.Id, "Empresa Atualizada", null, carregado!.Empresa.Cnpj, null, null, null, null, null,
            null, null, null, null, null, null, null,
            ConfirmarCancelamentoContratos: true, ContratosConfirmados: [contrato.Id]));

        Assert.True(confirmado.Sucesso, confirmado.Erro);
        Assert.Equal(Officio.Dominio.Enumeradores.StatusContrato.Cancelado,
            (await contexto.Contracts.SingleAsync(c => c.Id == contrato.Id)).Status);
        Assert.Equal("Empresa Atualizada", (await contexto.Empresas.SingleAsync(e => e.Id == empresa.Id)).RazaoSocial);
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
        => new(new ClienteRepositorio(contexto), new ServicoEnderecoFake(), publicador,
            new ProspectoRepositorio(contexto), new ServicoAssinaturaFake());

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
        {
            var endereco = string.IsNullOrWhiteSpace(requisicao.Logradouro)
                ? null
                : Endereco.Criar(requisicao.MunicipioId, requisicao.Logradouro, requisicao.Cep,
                    requisicao.Numero, requisicao.Bairro, requisicao.Complemento);
            return Task.FromResult(Resultado<Endereco?>.CriarSucesso(endereco));
        }

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

    private sealed class ServicoAssinaturaFake : IServicoAssinatura
    {
        public Task<ResultadoBloqueioAssinatura> BloquearDocumentoAsync(string documentoId, CancellationToken ct = default)
            => Task.FromResult(new ResultadoBloqueioAssinatura(true));
        public Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(SolicitacaoAssinatura request, CancellationToken ct = default)
            => Task.FromResult(ResultadoAssinatura.CriarFalha("Nao usado."));
        public Task<ResultadoEstadoAssinatura> ConsultarEstadoDocumentoAsync(string documentoId, CancellationToken ct = default)
            => Task.FromResult(ResultadoEstadoAssinatura.CriarSucesso(false));
        public Task<ResultadoDocumentoAssinado> BaixarDocumentoAssinadoAsync(string documentoId, CancellationToken ct = default)
            => Task.FromResult(ResultadoDocumentoAssinado.CriarFalha("Nao usado."));
    }
}
