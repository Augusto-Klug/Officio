using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Officio.Dominio.Enumeradores;

namespace Officio.Infraestrutura.Servicos;

internal static class NormalizadorCamposOcr
{
    private static readonly IReadOnlyDictionary<string, string[]> Alias =
        new Dictionary<string, string[]>
        {
            ["nome"] = ["nome", "nomecompleto", "nomecivil", "nomedoeleitor", "fullname", "name"],
            ["cpf"] = ["cpf", "cadastrodepessoafisica", "personalnumber", "taxid"],
            ["rg"] = ["rg", "registrogeral", "identitynumber"],
            ["data_nascimento"] = ["datadenascimento", "nascimento", "dateofbirth"],
            ["razao_social"] = ["razaosocial", "nomeempresarial", "companyname", "legalname"],
            ["nome_fantasia"] = ["nomefantasia", "tradename"],
            ["cnpj"] = ["cnpj", "cadastronacionaldapessoajuridica"],
            ["email"] = ["email", "enderecodeemail"],
            ["validade"] = ["validade", "datadevalidade", "expirydate", "expirationdate"],
            ["numero_titulo"] = ["numerodotitulo", "numeroinscricao", "inscricao", "inscricaoeleitoral"],
            ["numero_reservista"] = ["numerodocertificado", "certificadoreservista", "registrodealistamento"],
            ["escolaridade"] = ["escolaridade", "niveldeescolaridade", "graudeinstrucao"],
            ["instituicao"] = ["instituicao", "instituicaodeensino", "estabelecimentodeensino"],
            ["curso"] = ["curso", "nomedocurso"],
            ["cep"] = ["cep", "codigodeenderecamentopostal"],
            ["endereco"] = ["endereco", "logradouro", "address"]
        };

    public static ResultadoCamposOcr Normalizar(TipoDocumento tipo, IEnumerable<CampoOcrBruto> campos)
    {
        var camposMaterializados = campos
            .Where(item => !string.IsNullOrWhiteSpace(item.Chave) && !string.IsNullOrWhiteSpace(item.Valor))
            .ToList();
        var linhas = camposMaterializados
            .Where(item => item.Chave.StartsWith("__linha_", StringComparison.Ordinal))
            .ToList();
        var origem = camposMaterializados
            .Where(item => !item.Chave.StartsWith("__linha_", StringComparison.Ordinal))
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
            {
                var digitos = string.Concat(numeroDocumento.Valor.Where(char.IsDigit));
                Adicionar(
                    valores,
                    confiancas,
                    digitos.Length == 11 ? "cpf" : "rg",
                    numeroDocumento.Valor,
                    numeroDocumento.Confianca);
            }
        }

        ExtrairTextoLivre(tipo, linhas, camposMaterializados, valores, confiancas);

        NormalizarDocumentoFiscal(valores, "cpf");
        NormalizarDocumentoFiscal(valores, "cnpj");
        NormalizarData(valores, "data_nascimento");

        return new(valores, confiancas);
    }

    private static void ExtrairTextoLivre(
        TipoDocumento tipo,
        IReadOnlyList<CampoOcrBruto> linhas,
        IReadOnlyCollection<CampoOcrBruto> camposMaterializados,
        IDictionary<string, string> valores,
        IDictionary<string, decimal> confiancas)
    {
        var tipoDetectado = DetectarTipoDocumento(linhas, valores, camposMaterializados);
        if (tipoDetectado is not null)
            Adicionar(valores, confiancas, "tipo_documento_detectado", tipoDetectado, ConfiancaMedia(linhas));

        if (linhas.Count == 0)
            return;

        var texto = string.Join('\n', linhas.Select(item => item.Valor));
        if (tipo is TipoDocumento.Cpf or TipoDocumento.CpfSocio or
            TipoDocumento.Identidade or TipoDocumento.RgSocio or TipoDocumento.CnhSocio or
            TipoDocumento.Cnh or TipoDocumento.TituloEleitor or TipoDocumento.CarteiraTrabalho or
            TipoDocumento.CertificadoReservista)
        {
            var cpf = Regex.Match(texto, @"(?<!\d)\d{3}\.?\d{3}\.?\d{3}-?\d{2}(?!\d)");
            if (cpf.Success)
                Adicionar(valores, confiancas, "cpf", cpf.Value, ConfiancaMedia(linhas));
        }

        if (tipo is TipoDocumento.CartaoCnpj or TipoDocumento.ContratoSocial)
        {
            var cnpj = Regex.Match(texto, @"(?<!\d)\d{2}\.?\d{3}\.?\d{3}/?\d{4}-?\d{2}(?!\d)");
            if (cnpj.Success)
                Adicionar(valores, confiancas, "cnpj", cnpj.Value, ConfiancaMedia(linhas));
        }

        if (tipo is TipoDocumento.Identidade or TipoDocumento.RgSocio or TipoDocumento.CnhSocio or
            TipoDocumento.Cnh or TipoDocumento.Cpf or TipoDocumento.CpfSocio or
            TipoDocumento.ComprovanteResidencia or TipoDocumento.CarteiraTrabalho or
            TipoDocumento.TituloEleitor or TipoDocumento.ComprovanteEscolaridade or
            TipoDocumento.CertificadoReservista)
        {
            var nome = ExtrairValorAposRotulos(
                linhas,
                "nome",
                "nomecompleto",
                "nomecivil",
                "nomedoeleitor",
                "nomedotitular",
                "nomedoaluno");
            if (nome is null && tipo is TipoDocumento.Identidade or TipoDocumento.RgSocio or
                TipoDocumento.CnhSocio or TipoDocumento.Cnh)
                nome = ExtrairNomeEmFormatoNatural(linhas);
            if (nome is not null)
                Adicionar(valores, confiancas, "nome", nome.Valor, nome.Confianca);
        }

        if (tipo == TipoDocumento.ComprovanteResidencia)
        {
            var cep = Regex.Match(texto, @"(?<!\d)\d{5}-?\d{3}(?!\d)");
            if (cep.Success)
                Adicionar(valores, confiancas, "cep", cep.Value, ConfiancaMedia(linhas));
        }

        if (tipo == TipoDocumento.TituloEleitor)
            AdicionarValorRotulado(valores, confiancas, linhas, "numero_titulo",
                "numerodotitulo", "numeroinscricao", "inscricao", "inscricaoeleitoral");

        if (tipo == TipoDocumento.CertificadoReservista)
            AdicionarValorRotulado(valores, confiancas, linhas, "numero_reservista",
                "numerodocertificado", "certificadoreservista", "registrodealistamento");

        if (tipo == TipoDocumento.ComprovanteEscolaridade)
        {
            AdicionarValorRotulado(valores, confiancas, linhas, "instituicao",
                "instituicao", "instituicaodeensino", "estabelecimentodeensino");
            AdicionarValorRotulado(valores, confiancas, linhas, "curso", "curso", "nomedocurso");
            AdicionarValorRotulado(valores, confiancas, linhas, "escolaridade",
                "escolaridade", "niveldeescolaridade", "graudeinstrucao");
        }

    }

    private static string? DetectarTipoDocumento(
        IReadOnlyList<CampoOcrBruto> linhas,
        IDictionary<string, string> valores,
        IReadOnlyCollection<CampoOcrBruto> campos)
    {
        var texto = NormalizarFrase(string.Join(' ', campos.SelectMany(item => new[] { item.Chave, item.Valor })));
        var tipoAzure = campos
            .Where(item => NormalizarChave(item.Chave) is "documenttype" or "typedocumento" or "tipodedocumento")
            .Select(item => NormalizarFrase(item.Valor))
            .FirstOrDefault();

        var textoLinhas = NormalizarFrase(string.Join(' ', linhas.Select(item => item.Valor)));
        var endereco = valores.ContainsKey("endereco") || valores.ContainsKey("cep") ||
            Regex.IsMatch(textoLinhas, @"\bCEP\b\s*\d{5}-?\d{3}");
        var indicioContaResidencial = ContemAlgum(texto,
            "FATURA", "CONTA DE ENERGIA", "ENERGIA ELETRICA", "UNIDADE CONSUMIDORA",
            "CELESC", "CASAN", "CONTA DE AGUA", "SANEAMENTO", "SERVICO DE TELEFONIA",
            "VALOR TOTAL A PAGAR");
        if (endereco && indicioContaResidencial)
            return "COMPROVANTE_RESIDENCIA";

        var tipoAzureDetectado = tipoAzure switch
        {
            "DRIVER LICENSE" or "DRIVERLICENSE" or "CNH" => "CNH",
            "ID CARD" or "IDCARD" or "IDENTITY CARD" or "IDENTITYCARD" or "ID" => "IDENTIDADE",
            _ => null
        };
        if (tipoAzureDetectado is not null)
            return tipoAzureDetectado;

        var candidatos = new HashSet<string>(StringComparer.Ordinal);
        if (ContemAlgum(texto,
                "CARTEIRA NACIONAL DE HABILITACAO", "PERMISSAO PARA DIRIGIR", "CNH") ||
            (ContemAlgum(texto, "CATEGORIA") && ContemAlgum(texto, "HABILITACAO")))
            candidatos.Add("CNH");
        if (ContemAlgum(texto,
                "CARTEIRA DE IDENTIDADE", "REGISTRO GERAL", "DOCUMENTO DE IDENTIDADE",
                "CARTEIRA DE IDENTIDADE NACIONAL", "IDENTIDADE NACIONAL") ||
            Regex.IsMatch(texto, @"(^| )RG($| )"))
            candidatos.Add("IDENTIDADE");
        if (ContemAlgum(texto, "TITULO ELEITORAL", "TITULO DE ELEITOR", "ZONA ELEITORAL", "SECAO ELEITORAL") ||
            (valores.TryGetValue("numero_titulo", out var numeroTitulo) &&
             string.Concat(numeroTitulo.Where(char.IsDigit)).Length == 12))
            candidatos.Add("TITULO_ELEITOR");
        if (ContemAlgum(texto,
                "CARTEIRA DE TRABALHO", "CARTEIRA DE TRABALHO DIGITAL", "CTPS", "CONTRATO DE TRABALHO"))
            candidatos.Add("CARTEIRA_TRABALHO");
        if (ContemAlgum(texto,
                "CERTIFICADO DE RESERVISTA", "CERTIFICADO DE ALISTAMENTO MILITAR", "SERVICO MILITAR") ||
            valores.ContainsKey("numero_reservista"))
            candidatos.Add("CERTIFICADO_RESERVISTA");
        if (ContemAlgum(texto,
                "HISTORICO ESCOLAR", "DIPLOMA", "CERTIFICADO DE CONCLUSAO", "INSTITUICAO DE ENSINO") ||
            valores.ContainsKey("instituicao") || valores.ContainsKey("curso"))
            candidatos.Add("COMPROVANTE_ESCOLARIDADE");

        return candidatos.Count == 1 ? candidatos.Single() : null;
    }

    private static bool ContemAlgum(string texto, params string[] termos)
        => termos.Any(termo => texto.Contains(termo, StringComparison.Ordinal));

    private static string NormalizarFrase(string valor)
    {
        var decomposicao = valor.Normalize(NormalizationForm.FormD);
        var semAcentos = new StringBuilder(decomposicao.Length);
        foreach (var caractere in decomposicao)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark)
                continue;
            semAcentos.Append(char.ToUpperInvariant(caractere));
        }

        return Regex.Replace(semAcentos.ToString(), @"[^A-Z0-9]+", " ").Trim();
    }

    private static void AdicionarValorRotulado(
        IDictionary<string, string> valores,
        IDictionary<string, decimal> confiancas,
        IReadOnlyList<CampoOcrBruto> linhas,
        string campo,
        params string[] rotulos)
    {
        var encontrado = ExtrairValorAposRotulos(linhas, rotulos);
        if (encontrado is not null)
            Adicionar(valores, confiancas, campo, encontrado.Valor, encontrado.Confianca);
    }

    private static CampoOcrBruto? ExtrairValorAposRotulos(
        IReadOnlyList<CampoOcrBruto> linhas,
        params string[] rotulos)
    {
        for (var indice = 0; indice < linhas.Count; indice++)
        {
            var linha = linhas[indice];
            var chave = NormalizarChave(linha.Valor);
            foreach (var rotulo in rotulos)
            {
                if (chave == rotulo && indice < linhas.Count - 1)
                    return linhas[indice + 1];

                if (!chave.StartsWith(rotulo, StringComparison.Ordinal) || chave.Length == rotulo.Length)
                    continue;

                var separador = linha.Valor.IndexOfAny([':', '-']);
                if (separador >= 0 && separador < linha.Valor.Length - 1)
                    return linha with { Valor = linha.Valor[(separador + 1)..].Trim() };
            }
        }

        return null;
    }

    private static CampoOcrBruto? ExtrairNomeEmFormatoNatural(IReadOnlyList<CampoOcrBruto> linhas)
        => linhas.FirstOrDefault(item =>
        {
            var palavras = item.Valor.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return palavras.Length is >= 2 and <= 6 &&
                   palavras.All(palavra => palavra.Length >= 2 && char.IsUpper(palavra[0]) &&
                       palavra.Skip(1).Any(char.IsLower));
        });

    private static decimal ConfiancaMedia(IReadOnlyList<CampoOcrBruto> linhas)
        => linhas.Count == 0 ? 0m : linhas.Average(item => item.Confianca);

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
