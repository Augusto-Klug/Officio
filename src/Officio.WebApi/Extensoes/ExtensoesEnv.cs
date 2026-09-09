namespace Officio.WebApi.Extensoes;

public static class ExtensoesEnv
{
    public static IConfigurationBuilder AdicionarArquivoEnv(
        this IConfigurationBuilder configuration,
        IWebHostEnvironment environment)
    {
        var caminho = EncontrarArquivoEnv(environment.ContentRootPath);
        if (caminho is null)
            return configuration;

        var variaveis = File.ReadLines(caminho)
            .Select(LerLinha)
            .Where(par => par is not null)
            .Select(par => par!.Value)
            .ToDictionary(
                par => par.Chave.Replace("__", ":", StringComparison.Ordinal),
                par => (string?)par.Valor,
                StringComparer.OrdinalIgnoreCase);

        configuration.AddInMemoryCollection(variaveis);

        return configuration;
    }

    private static string? EncontrarArquivoEnv(string contentRootPath)
    {
        var diretorio = new DirectoryInfo(contentRootPath);

        while (diretorio is not null)
        {
            var caminho = Path.Combine(diretorio.FullName, ".env");
            if (File.Exists(caminho))
                return caminho;

            diretorio = diretorio.Parent;
        }

        return null;
    }

    private static (string Chave, string Valor)? LerLinha(string linha)
    {
        var conteudo = linha.Trim();
        if (conteudo.Length == 0 || conteudo.StartsWith('#'))
            return null;

        var indiceSeparador = conteudo.IndexOf('=');
        if (indiceSeparador <= 0)
            return null;

        var chave = conteudo[..indiceSeparador].Trim();
        var valor = conteudo[(indiceSeparador + 1)..].Trim();

        if (valor.Length >= 2 &&
            ((valor.StartsWith('"') && valor.EndsWith('"')) ||
             (valor.StartsWith('\'') && valor.EndsWith('\''))))
            valor = valor[1..^1];

        return (chave, valor);
    }
}
