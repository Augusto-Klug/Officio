using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class EmpresaMapeamentoTestes
{
    [Fact]
    public void Empresas_PreservaTabelaLegadaSemTenantId()
    {
        using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlServer("Server=localhost;Database=TesteModelagem;Integrated Security=true").Options,
            new ServicoEmpresaAtual(), new PublicadorEventosFake());
        var empresa = db.Model.FindEntityType(typeof(Empresa))!;
        Assert.Equal("Tenants", empresa.GetTableName());
        Assert.Null(empresa.FindProperty("TenantId"));
        var vinculo = db.Model.FindEntityType(typeof(UsuarioEmpresa))!;
        Assert.Contains(vinculo.GetForeignKeys(), fk => fk.PrincipalEntityType == empresa &&
            fk.Properties.Single().Name == nameof(UsuarioEmpresa.EmpresaId));
    }
}
