using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;
using Officio.Testes.Integracao.Infraestrutura;
using MediatR;
namespace Officio.Testes.Unitarios.Dominio;
public class CadastroRepresentanteTestes
{
    [Theory]
    [InlineData("Nome")]
    [InlineData("CPF")]
    [InlineData("Profissão")]
    [InlineData("Tipo de relação")]
    [InlineData("CEP")]
    [InlineData("Logradouro")]
    [InlineData("Número")]
    [InlineData("Bairro")]
    [InlineData("Município")]
    [InlineData("UF")]
    public async Task CampoObrigatorioAusente_ImpedeCadastroCompleto(string campo)
    {
        var tenant = Guid.NewGuid(); var atual = new ServicoEmpresaAtual(); atual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, atual, new Publicador());
        var p = Prospecto.Create(tenant, "Empresa", null, null);
        var s = SocioContratualFixture.Adicionar(db, p);
        await db.SaveChangesAsync();
        await db.Entry(s.Pessoa.Endereco!).Reference(e => e.Municipio).LoadAsync();
        var e = s.Pessoa.Endereco!;
        Assert.Empty(CadastroRepresentante.InformacoesFaltantes(s));
        s.Pessoa.AtualizarDadosSocietarios(campo == "Nome" ? null : s.Nome, campo == "CPF" ? null : s.Cpf,
            campo == "Profissão" ? null : s.Pessoa.Profissao, e);
        if (campo == "Tipo de relação") s.AtualizarVinculo(null, null);
        e.AtualizarParcial(campo == "Município" ? null : e.MunicipioId,
            campo == "UF" ? Guid.NewGuid() : null,
            campo == "Logradouro" ? null : e.Logradouro, campo == "CEP" ? null : e.Cep,
            campo == "Número" ? null : e.Numero, campo == "Bairro" ? null : e.Bairro, null);
        Assert.Contains(CadastroRepresentante.InformacoesFaltantes(s), f => f.StartsWith(campo));
    }
    private class Publicador : IPublisher
    {
        public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
        public Task Publish<T>(T notification, CancellationToken ct = default) where T : INotification => Task.CompletedTask;
    }
}
