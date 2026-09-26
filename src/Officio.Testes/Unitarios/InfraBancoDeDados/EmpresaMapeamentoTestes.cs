using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class EmpresaMapeamentoTestes
{
    [Fact]
    public void ParceirosEEmpresas_PossuemTabelasERelacionamentoDistintos()
    {
        using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlServer("Server=localhost;Database=TesteModelagem;Integrated Security=true").Options,
            new ServicoEmpresaAtual(), new PublicadorEventosFake());
        var empresa = db.Model.FindEntityType(typeof(Empresa))!;
        var parceiro = db.Model.FindEntityType(typeof(Parceiro))!;

        Assert.Equal("Parceiros", parceiro.GetTableName());
        Assert.Equal("Empresas", empresa.GetTableName());
        Assert.Equal("ParceiroId", empresa.FindProperty(nameof(Empresa.TenantId))!.GetColumnName());
        Assert.Contains(empresa.GetForeignKeys(), fk => fk.PrincipalEntityType == parceiro &&
            fk.Properties.Single().Name == nameof(Empresa.TenantId));

        var vinculo = db.Model.FindEntityType(typeof(UsuarioEmpresa))!;
        Assert.Contains(vinculo.GetForeignKeys(), fk => fk.PrincipalEntityType == empresa &&
            fk.Properties.Single().Name == nameof(UsuarioEmpresa.EmpresaId));
    }
}
