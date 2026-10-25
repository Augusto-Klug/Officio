namespace Officio.Infraestrutura.Pdf.Formatacao;

internal static class NumeroPorExtenso
{
    private static readonly string[] Unidades = ["zero", "um", "dois", "três", "quatro", "cinco", "seis", "sete", "oito", "nove", "dez", "onze", "doze", "treze", "catorze", "quinze", "dezesseis", "dezessete", "dezoito", "dezenove"];
    private static readonly string[] Dezenas = ["", "", "vinte", "trinta", "quarenta", "cinquenta", "sessenta", "setenta", "oitenta", "noventa"];
    private static readonly string[] Centenas = ["", "cento", "duzentos", "trezentos", "quatrocentos", "quinhentos", "seiscentos", "setecentos", "oitocentos", "novecentos"];
    private static readonly (string Singular, string Plural)[] Escalas = [("", ""), ("mil", "mil"), ("milhão", "milhões"), ("bilhão", "bilhões"), ("trilhão", "trilhões"), ("quadrilhão", "quadrilhões"), ("quintilhão", "quintilhões")];

    public static string Inteiro(long valor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valor);
        if (valor < 1000) return Grupo((int)valor);
        var grupos = new List<(int Numero, string Texto)>();
        for (var escala = 0; valor > 0; escala++, valor /= 1000)
        {
            var grupo = (int)(valor % 1000);
            if (grupo == 0) continue;
            var texto = escala == 1 && grupo == 1 ? "mil" : Grupo(grupo);
            if (escala > 0 && !(escala == 1 && grupo == 1))
                texto += " " + (grupo == 1 ? Escalas[escala].Singular : Escalas[escala].Plural);
            grupos.Insert(0, (grupo, texto));
        }
        var resultado = grupos[0].Texto;
        for (var i = 1; i < grupos.Count; i++)
        {
            var ultimo = i == grupos.Count - 1;
            var conjuncao = ultimo && (grupos[i].Numero < 100 || grupos[i].Numero % 100 == 0);
            resultado += (conjuncao ? " e " : " ") + grupos[i].Texto;
        }
        return resultado;
    }

    public static string Moeda(decimal valor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valor);
        valor = decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
        var reais = checked((long)decimal.Truncate(valor));
        var centavos = (int)((valor - reais) * 100);
        var partes = new List<string>();
        if (reais > 0 || centavos == 0)
            partes.Add(Inteiro(reais) + (reais == 1 ? " real" : reais > 0 && reais % 1_000_000 == 0 ? " de reais" : " reais"));
        if (centavos > 0)
            partes.Add(Inteiro(centavos) + (centavos == 1 ? " centavo" : " centavos"));
        return string.Join(" e ", partes);
    }

    private static string Grupo(int valor)
    {
        if (valor < 20) return Unidades[valor];
        if (valor < 100) return Dezenas[valor / 10] + (valor % 10 == 0 ? "" : " e " + Unidades[valor % 10]);
        if (valor == 100) return "cem";
        return Centenas[valor / 100] + (valor % 100 == 0 ? "" : " e " + Grupo(valor % 100));
    }
}
