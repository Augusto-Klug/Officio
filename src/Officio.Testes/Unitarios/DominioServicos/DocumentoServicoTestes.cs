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

namespace Officio.Testes.Unitarios.DominioServicos;

public class DocumentoServicoTestes
{
    [Fact]
    public async Task ValidarAsync_QuandoDocumentoDivergeDoCadastro_RegistraDivergencia()
    {
        var tenantId = Guid.NewGuid();
        var cliente = Cliente.CreateFromProspect(Prospecto.Create(tenantId, "Empresa Certa Ltda", "11222333000144", null));
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.85m, clientId: cliente.Id);

        await using var provedor = CriarProvedor(tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["razao_social"] = "Empresa Errada Ltda",
                ["cnpj"] = "11222333000144"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
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
    public async Task ValidarAsync_QuandoSimilaridadeAcimaDoLimite_Aprova()
    {
        var tenantId = Guid.NewGuid();
        var cliente = Cliente.CreateFromProspect(Prospecto.Create(tenantId, "Empresa Certa Ltda", "11222333000144", null));
        var documento = Documento.Create(tenantId, "cartao-cnpj.pdf", TipoDocumento.CartaoCnpj, "doc.pdf", 0.85m, clientId: cliente.Id);

        await using var provedor = CriarProvedor(tenantId,
            ResultadoOcr.CriarSucesso(new Dictionary<string, string>
            {
                ["razao_social"] = "Empresa Certa Ltda",
                ["cnpj"] = "11222333000144"
            }, 0.95m));
        var contexto = provedor.GetRequiredService<ContextoAplicacao>();
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
    }

    private static ServiceProvider CriarProvedor(Guid tenantId, ResultadoOcr resultadoOcr)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);

        var servicos = new ServiceCollection();
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<IServicoOcr>(new ServicoOcrFake(resultadoOcr));
        servicos.AddSingleton<IServicoArmazenamento, ServicoArmazenamentoFake>();
        servicos.AddSingleton<PublicadorEventosFake>();
        servicos.AddSingleton<MediatR.IPublisher>(sp => sp.GetRequiredService<PublicadorEventosFake>());
        servicos.AddDbContext<ContextoAplicacao>(opcoes => opcoes.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        servicos.AddScoped<DbContext>(sp => sp.GetRequiredService<ContextoAplicacao>());
        servicos.AddScoped<IDocumentoRepositorio, DocumentoRepositorio>();
        servicos.AdicionarServicosDominio(new ConfigurationBuilder().Build());

        return servicos.BuildServiceProvider();
    }

    private sealed class ServicoOcrFake(ResultadoOcr resultado) : IServicoOcr
    {
        public Task<ResultadoOcr> AnalisarDocumentoAsync(Stream documentStream, TipoDocumento type, CancellationToken ct = default)
            => Task.FromResult(resultado);
    }

    private sealed class ServicoArmazenamentoFake : IServicoArmazenamento
    {
        public Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
            => Task.FromResult(fileName);

        public Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
            => Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));

        public Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default)
            => Task.FromResult(path);
    }
}
