using Officio.Dominio.Entidades;

namespace Officio.Dominio.Comum;

public static class CadastroEndereco
{
    public static IReadOnlyList<string> InformacoesFaltantes(Endereco? endereco)
    {
        var municipio = endereco?.Municipio;
        var municipioValido = municipio is not null && endereco!.MunicipioId == municipio.Id;
        var estado = municipio?.Estado;
        var ufValida = estado is not null && !string.IsNullOrWhiteSpace(estado.Sigla)
            && (!endereco!.EstadoId.HasValue || endereco.EstadoId == estado.Id);

        return Avaliar(
            endereco?.Cep,
            endereco?.Logradouro,
            endereco?.Numero,
            endereco?.Bairro,
            municipioValido,
            ufValida);
    }

    public static IReadOnlyList<string> InformacoesFaltantes(Prospecto prospecto)
    {
        var municipio = prospecto.Municipio;
        var municipioValido = prospecto.MunicipioId.HasValue
            ? municipio is not null && municipio.Id == prospecto.MunicipioId
            : !string.IsNullOrWhiteSpace(prospecto.Cidade);
        var ufDaRelacao = municipio?.Estado?.Sigla;
        var ufValida = !string.IsNullOrWhiteSpace(ufDaRelacao)
            ? string.IsNullOrWhiteSpace(prospecto.Uf)
                || string.Equals(prospecto.Uf.Trim(), ufDaRelacao, StringComparison.OrdinalIgnoreCase)
            : !string.IsNullOrWhiteSpace(prospecto.Uf);

        return Avaliar(
            prospecto.Cep,
            prospecto.Logradouro,
            prospecto.Numero,
            prospecto.Bairro,
            municipioValido,
            ufValida);
    }

    private static IReadOnlyList<string> Avaliar(
        string? cep,
        string? logradouro,
        string? numero,
        string? bairro,
        bool municipioValido,
        bool ufValida)
    {
        var faltantes = new List<string>();
        if (DocumentoFiscal.SomenteDigitos(cep)?.Length != 8) faltantes.Add("CEP válido");
        if (string.IsNullOrWhiteSpace(logradouro)) faltantes.Add("Logradouro");
        if (string.IsNullOrWhiteSpace(numero)) faltantes.Add("Número");
        if (string.IsNullOrWhiteSpace(bairro)) faltantes.Add("Bairro");
        if (!municipioValido) faltantes.Add("Município");
        if (!ufValida) faltantes.Add("UF consistente");
        return faltantes;
    }
}
