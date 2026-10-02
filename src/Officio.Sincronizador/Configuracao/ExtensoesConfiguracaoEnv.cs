using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Officio.Sincronizador.Configuracao;

internal static class ExtensoesConfiguracaoEnv
{
    public static IConfigurationBuilder AdicionarArquivoEnv(
        this IConfigurationBuilder configuration,
        IHostEnvironment environment)
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

        var indice = configuration.Sources.Count;
        for (var i = 0; i < configuration.Sources.Count; i++)
        {
            if (configuration.Sources[i] is EnvironmentVariablesConfigurationSource)
            {
                indice = i;
                break;
            }
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
