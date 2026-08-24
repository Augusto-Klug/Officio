using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class MapeamentoModelagemOfficioTestes
{
    [Fact]
    public void Modelo_DeveConterIndicesCriticosDaNovaModelagem()
    {
        using var contexto = CriarContexto();
        var modelo = contexto.Model;

        Assert.True(PossuiIndice(modelo.FindEntityType(typeof(Municipio))!, [nameof(Municipio.CodigoIbge)]));
        Assert.True(PossuiIndice(modelo.FindEntityType(typeof(Pessoa))!, [nameof(Pessoa.Cpf)]));
        Assert.True(PossuiIndice(modelo.FindEntityType(typeof(Empresa))!, [nameof(Empresa.Cnpj)]));
        Assert.True(PossuiIndice(modelo.FindEntityType(typeof(ProspectoPessoa))!, [
            nameof(ProspectoPessoa.ProspectId),
            nameof(ProspectoPessoa.PessoaId),
            nameof(ProspectoPessoa.TipoRelacaoId)]));
        Assert.True(PossuiIndice(modelo.FindEntityType(typeof(FaturamentoMensal))!, [
            nameof(FaturamentoMensal.TenantId),
            nameof(FaturamentoMensal.ContratoId),
            nameof(FaturamentoMensal.Competencia)]));
    }

    [Fact]
    public void Modelo_DeveConterRelacionamentosCentrais()
    {
        using var contexto = CriarContexto();
        var modelo = contexto.Model;

        Assert.Contains(modelo.FindEntityType(typeof(Prospecto))!.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == nameof(Prospecto.EmpresaId)));
        Assert.Contains(modelo.FindEntityType(typeof(Cliente))!.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == nameof(Cliente.EmpresaId)));
        Assert.Contains(modelo.FindEntityType(typeof(Contrato))!.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == nameof(Contrato.OrcamentoVersaoAceitaId)));
        Assert.Contains(modelo.FindEntityType(typeof(ValidacaoDocumento))!.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == nameof(ValidacaoDocumento.OcrExecucaoId)));
    }

    private static ContextoAplicacao CriarContexto()
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }

    private static bool PossuiIndice(Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType, string[] propriedades)
        => entityType
            .GetIndexes()
            .Any(indice => indice.Properties.Select(propriedade => propriedade.Name).SequenceEqual(propriedades));
}
