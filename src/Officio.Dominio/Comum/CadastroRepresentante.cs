using Officio.Dominio.Entidades;
namespace Officio.Dominio.Comum;

public static class CadastroRepresentante
{
    public static IReadOnlyList<string> InformacoesFaltantes(Socio socio)
    {
        var faltantes = new List<string>();
        var pessoa = socio.Pessoa;
        if (string.IsNullOrWhiteSpace(pessoa.Nome)) faltantes.Add("Nome");
        if (DocumentoFiscal.SomenteDigitos(pessoa.Cpf)?.Length != 11) faltantes.Add("CPF válido");
        if (string.IsNullOrWhiteSpace(pessoa.Profissao)) faltantes.Add("Profissão");
        if (socio.TipoRelacao is not { Ativo: true } tipo || (tipo.Codigo != "SOCIO" && tipo.Codigo != "SOCIO_ADMINISTRADOR"))
            faltantes.Add("Tipo de relação");
        faltantes.AddRange(CadastroEndereco.InformacoesFaltantes(pessoa.Endereco));
        return faltantes;
    }
}
