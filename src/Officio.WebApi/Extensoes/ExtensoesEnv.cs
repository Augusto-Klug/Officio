using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesEnv
{
    public static IConfigurationBuilder AdicionarArquivoEnv(
        this IConfigurationBuilder configuration,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return configuration;

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

        // Acima de appsettings, abaixo de user-secrets, ambiente e argumentos.
        // Nao reposicionar os demais providers nem confundir secrets.json com appsettings.
        var indice = 0;
        for (var i = 0; i < configuration.Sources.Count; i++)
        {
            if (configuration.Sources[i] is JsonConfigurationSource json &&
                (json.Path == "appsettings.json" || json.Path == $"appsettings.{environment.EnvironmentName}.json"))
                indice = i + 1;
        }
        configuration.Sources.Insert(indice, new MemoryConfigurationSource { InitialData = variaveis });

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
