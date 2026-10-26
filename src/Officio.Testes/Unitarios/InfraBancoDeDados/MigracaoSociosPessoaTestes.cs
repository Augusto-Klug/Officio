using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Officio.Infra.BancoDeDados.Migrations;
namespace Officio.Testes.Unitarios.InfraBancoDeDados;
public class MigracaoSociosPessoaTestes
{
    [Fact]
    public void Up_CopiaDadosAntesDeRemoverLegadoEExigirPessoa()
    {
        var b = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        new Exposta().Aplicar(b);
        var operacoes = b.Operations.ToList();
        var copia = Assert.Single(operacoes.OfType<SqlOperation>(), s => s.Sql.Contains("INSERT INTO Pessoas"));
        Assert.Contains("s.Nome", copia.Sql);
        Assert.Contains("s.Cpf", copia.Sql);
        Assert.Contains("ProspectOrigemId", copia.Sql);
        Assert.Contains("sp.TenantId = sc.TenantId", copia.Sql);
        Assert.Contains("HAVING COUNT(*) = 1", copia.Sql);
        Assert.Contains("SOCIO_ADMINISTRADOR", copia.Sql);
        Assert.Contains("'SOCIO'", copia.Sql);
        foreach (var coluna in operacoes.OfType<DropColumnOperation>().Where(o => o.Table == "Socios"))
            Assert.True(operacoes.IndexOf(copia) < operacoes.IndexOf(coluna));
        var pessoaObrigatoria = Assert.Single(operacoes.OfType<AlterColumnOperation>(), o => o.Name == "PessoaId");
        Assert.True(operacoes.IndexOf(copia) < operacoes.IndexOf(pessoaObrigatoria));
        Assert.False(pessoaObrigatoria.IsNullable);
        Assert.DoesNotContain(operacoes.OfType<DropTableOperation>(), o => o.Name == "Socios");
    }
    [Fact]
    public void Down_RecuperaNomeECpfAntesDeRemoverVinculo()
    {
        var b = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        new Exposta().Reverter(b);
        var ops = b.Operations.ToList();
        var restaurar = Assert.Single(ops.OfType<SqlOperation>(), s => s.Sql.Contains("SET Nome = p.Nome, Cpf = p.Cpf"));
        var remover = Assert.Single(ops.OfType<DropColumnOperation>(), s => s.Name == "PessoaId");
        Assert.True(ops.IndexOf(restaurar) < ops.IndexOf(remover));
    }
    private class Exposta : NormalizarSociosPessoa
    {
        public void Aplicar(MigrationBuilder b) => Up(b);
        public void Reverter(MigrationBuilder b) => Down(b);
    }
}
