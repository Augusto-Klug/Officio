using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;
using MediatR;

namespace Officio.Testes.Unitarios.Dominio;

public class ParceiroCadastroTestes
{
    [Fact]
    public void CriarEAtualizarCadastroLegado_MantemNovosDadosOpcionaisEPreservados()
    {
        var p = Parceiro.CriarDesenvolvimento(Guid.NewGuid(), "11.222.333/0001-44", "Contabilidade");
        Assert.Null(p.Crc); Assert.Null(p.Telefone); Assert.Null(p.EnderecoId); Assert.Null(p.ResponsavelTecnico);
        var enderecoId = Guid.NewGuid();
        p.AtualizarDadosContabilidade(p.Cnpj, p.RazaoSocial, null, null, "11999998888", "SP-1");
        p.VincularEndereco(enderecoId);
        p.AtualizarCadastro(p.Cnpj, "Cadastro pelo bootstrap", null, "contato@teste.test");
        Assert.Equal("SP-1", p.Crc);
        Assert.Equal("11999998888", p.Telefone);
        Assert.Equal(enderecoId, p.EnderecoId);
        Assert.Equal("11222333000144", p.Cnpj);
    }

    [Fact]
    public void Modelo_GaranteResponsavelUnicoPorParceiroEFksDeEndereco()
    {
        using var contexto = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlServer("Server=localhost;Database=SomenteModelo;Trusted_Connection=True;TrustServerCertificate=True").Options,
            new ServicoEmpresaAtual(), new PublicadorNulo());
        var entidade = contexto.Model.FindEntityType(typeof(ResponsavelTecnico))!;
        Assert.Equal("ResponsaveisTecnicos", entidade.GetTableName());
        Assert.Contains(entidade.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "ParceiroId");
        Assert.Contains(entidade.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(Endereco));
        Assert.Contains(contexto.Model.FindEntityType(typeof(Parceiro))!.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Endereco));
        Assert.NotEmpty(entidade.GetDeclaredQueryFilters());
        Assert.False(contexto.Database.HasPendingModelChanges());
        Assert.False(contexto.Database.HasPendingModelChanges());
    }

    private sealed class PublicadorNulo : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;
    }
}
