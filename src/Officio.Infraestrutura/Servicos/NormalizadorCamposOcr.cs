using System.Globalization;
using System.Text;
using Officio.Dominio.Enumeradores;

namespace Officio.Infraestrutura.Servicos;

internal static class NormalizadorCamposOcr
{
    private static readonly IReadOnlyDictionary<string, string[]> Alias =
        new Dictionary<string, string[]>
        {
            ["nome"] = ["nome", "nomecompleto", "fullname", "name"],
            ["cpf"] = ["cpf", "cadastrodepessoafisica", "personalnumber", "taxid"],
            ["rg"] = ["rg", "registrogeral", "identitynumber"],
            ["data_nascimento"] = ["datadenascimento", "nascimento", "dateofbirth"],
            ["razao_social"] = ["razaosocial", "nomeempresarial", "companyname", "legalname"],
            ["nome_fantasia"] = ["nomefantasia", "tradename"],
            ["cnpj"] = ["cnpj", "cadastronacionaldapessoajuridica"],
            ["email"] = ["email", "enderecodeemail"]
        };

    public static ResultadoCamposOcr Normalizar(TipoDocumento tipo, IEnumerable<CampoOcrBruto> campos)
    {
        var origem = campos
            .Where(item => !string.IsNullOrWhiteSpace(item.Chave) && !string.IsNullOrWhiteSpace(item.Valor))
            .GroupBy(item => NormalizarChave(item.Chave))
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.OrderByDescending(item => item.Confianca).First(),
                StringComparer.OrdinalIgnoreCase);

        var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var confiancas = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var (campoCanonico, aliases) in Alias)
        {
            var encontrado = aliases
                .Select(alias => origem.GetValueOrDefault(alias))
                .FirstOrDefault(item => item is not null);
            if (encontrado is not null)
                Adicionar(valores, confiancas, campoCanonico, encontrado.Valor, encontrado.Confianca);
        }

        if (!valores.ContainsKey("nome"))
        {
            var primeiroNome = origem.GetValueOrDefault("firstname");
            var sobrenome = origem.GetValueOrDefault("lastname");
            var nome = string.Join(' ', new[] { primeiroNome?.Valor, sobrenome?.Valor }
                .Where(valor => !string.IsNullOrWhiteSpace(valor)));
            if (!string.IsNullOrWhiteSpace(nome))
                Adicionar(
                    valores,
                    confiancas,
                    "nome",
                    nome,
                    new[] { primeiroNome?.Confianca, sobrenome?.Confianca }
                        .Where(item => item.HasValue)
                        .Min() ?? 0m);
        }

        var numeroDocumento = origem.GetValueOrDefault("documentnumber");
        if (numeroDocumento is not null)
        {
            if (tipo is TipoDocumento.Cpf or TipoDocumento.CpfSocio)
                Adicionar(valores, confiancas, "cpf", numeroDocumento.Valor, numeroDocumento.Confianca);
            else if (tipo is TipoDocumento.Identidade or TipoDocumento.RgSocio)
                Adicionar(valores, confiancas, "rg", numeroDocumento.Valor, numeroDocumento.Confianca);
        }

        NormalizarDocumentoFiscal(valores, "cpf");
        NormalizarDocumentoFiscal(valores, "cnpj");
        NormalizarData(valores, "data_nascimento");

        return new(valores, confiancas);
    }

    private static void Adicionar(
        IDictionary<string, string> valores,
        IDictionary<string, decimal> confiancas,
        string chave,
        string valor,
        decimal confianca)
    {
        if (valores.ContainsKey(chave))
            return;

        valores[chave] = valor.Trim();
        confiancas[chave] = Math.Clamp(confianca, 0m, 1m);
    }

    private static void NormalizarDocumentoFiscal(IDictionary<string, string> valores, string chave)
    {
        if (!valores.TryGetValue(chave, out var valor))
            return;

        var digitos = string.Concat(valor.Where(char.IsDigit));
        if (!string.IsNullOrWhiteSpace(digitos))
            valores[chave] = digitos;
    }

    private static void NormalizarData(IDictionary<string, string> valores, string chave)
    {
        if (!valores.TryGetValue(chave, out var valor))
            return;

        foreach (var cultura in new[] { CultureInfo.GetCultureInfo("pt-BR"), CultureInfo.InvariantCulture })
        {
            if (!DateTime.TryParse(valor, cultura, DateTimeStyles.AllowWhiteSpaces, out var data))
                continue;

            valores[chave] = data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return;
        }
    }

    private static string NormalizarChave(string valor)
    {
        var decomposicao = valor.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposicao.Length);
        foreach (var caractere in decomposicao)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(caractere))
                builder.Append(char.ToLowerInvariant(caractere));
        }

        return builder.ToString();
    }
}

internal sealed record CampoOcrBruto(string Chave, string Valor, decimal Confianca);

internal sealed record ResultadoCamposOcr(
    Dictionary<string, string> Valores,
    Dictionary<string, decimal> Confiancas);
