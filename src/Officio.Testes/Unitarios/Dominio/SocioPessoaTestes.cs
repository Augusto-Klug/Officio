using Officio.Dominio.Entidades;
namespace Officio.Testes.Unitarios.Dominio;
public class SocioPessoaTestes
{
    [Fact]
    public void Pessoa_AtualizaDadosSocietariosSemPerderContatos()
    {
        var pessoa = Pessoa.Criar("Ana", "11789299900", "47999999999", "ana@test.test");
        var endereco = Endereco.Criar(null, "Rua parcial");
        pessoa.AtualizarDadosSocietarios(" Ana revisada ", "117.892.999-00", " Arquiteta ", endereco);
        Assert.Equal("Ana revisada", pessoa.Nome);
        Assert.Equal("11789299900", pessoa.Cpf);
        Assert.Equal("Arquiteta", pessoa.Profissao);
        Assert.Equal(endereco.Id, pessoa.EnderecoId);
        Assert.Equal("47999999999", pessoa.Telefone);
        Assert.Equal("ana@test.test", pessoa.Email);
        pessoa.AtualizarDadosSocietarios(null, null, null, null);
        Assert.Equal("", pessoa.Nome);
        Assert.Null(pessoa.Cpf); Assert.Null(pessoa.EnderecoId);
        Assert.Equal("ana@test.test", pessoa.Email);
    }

    [Fact]
    public void CopiaSocio_CompartilhaIndividuoEPreservaVinculoSemDuplicarDadosPessoais()
    {
        var original = Socio.Create(Guid.NewGuid(), null, null, null, prospectId: Guid.NewGuid());
        var tipo = new TipoRelacao("SOCIO_ADMINISTRADOR", "Sócio administrador", "SOCIETARIO");
        original.AtualizarVinculo(35, tipo);
        var copia = Socio.CopyFor(original, Guid.NewGuid());
        Assert.Same(original.Pessoa, copia.Pessoa);
        Assert.Equal(original.PessoaId, copia.PessoaId);
        Assert.Equal(tipo.Id, copia.TipoRelacaoId);
        Assert.Equal(35, copia.Participacao);
        original.AtualizarDados("Nome atualizado", "11789299900", 40);
        Assert.Equal("Nome atualizado", copia.Nome);
        Assert.Equal("11789299900", copia.Cpf);
        Assert.Equal(35, copia.Participacao);
    }
}
