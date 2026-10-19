namespace Officio.Testes.Integracao.Infraestrutura;

internal static class ConfiguracaoIntegracaoReal
{
    private static readonly IReadOnlyDictionary<string, string> Chaves = new Dictionary<string, string>
    {
        ["AzureOcr__Endpoint"] = "AzureOcr:Endpoint",
        ["AzureOcr__Key"] = "AzureOcr:Key",
        ["ConnectionStrings__AzureBlobStorage"] = "ConnectionStrings:AzureBlobStorage"
    };

    public static Dictionary<string, string?> ObterConfiguracoes()
    {
        var valores = LerArquivoEnv();
        return Chaves.ToDictionary(
            item => item.Value,
            item => ValorAmbiente(item.Key) ?? valores.GetValueOrDefault(item.Key));
    }

    private static Dictionary<string, string> LerArquivoEnv()
    {
        var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var diretoriosVisitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var inicio in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var diretorio = new DirectoryInfo(inicio);
            while (diretorio is not null && diretoriosVisitados.Add(diretorio.FullName))
            {
                var caminho = Path.Combine(diretorio.FullName, "src", ".env");
                if (File.Exists(caminho))
                {
                    foreach (var linha in File.ReadLines(caminho))
                    {
                        var separador = linha.IndexOf('=');
                        if (separador <= 0)
                            continue;

                        var chave = linha[..separador].Trim();
                        if (!Chaves.ContainsKey(chave))
                            continue;

                        valores[chave] = linha[(separador + 1)..].Trim().Trim('"', '\'');
                    }

                    return valores;
                }

                diretorio = diretorio.Parent;
            }
        }

        return valores;
    }

    private static string? ValorAmbiente(string chave)
    {
        var valor = Environment.GetEnvironmentVariable(chave);
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
