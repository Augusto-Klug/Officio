using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class ClienteMapeamentoTestes
{
    [Fact]
    public void ProspectoOrigem_PossuiIndiceUnicoFiltradoPorTenant()
    {
        using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlServer("Server=localhost;Database=TesteModelagem;Integrated Security=true").Options,
            new ServicoEmpresaAtual(), new PublicadorEventosFake());

        var cliente = db.Model.FindEntityType(typeof(Cliente))!;
        var indice = Assert.Single(cliente.GetIndexes(), item =>
            item.Properties.Select(property => property.Name).SequenceEqual([
                nameof(Cliente.TenantId),
                nameof(Cliente.ProspectOrigemId)
            ]));

        Assert.True(indice.IsUnique);
        Assert.Equal("[ProspectOrigemId] IS NOT NULL", indice.GetFilter());
    }
}
