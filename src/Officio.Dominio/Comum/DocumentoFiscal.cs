namespace Officio.Dominio.Comum;

public static class DocumentoFiscal
{
    public static string? SomenteDigitos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var digitos = new string(valor.Where(char.IsDigit).ToArray());
        return digitos.Length == 0 ? null : digitos;
    }
}
