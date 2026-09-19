using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Officio.WebApi.Extensoes;

namespace Officio.Testes.Unitarios.WebApi;

public class ExtensoesEnvTestes : IDisposable
{
    private readonly string pasta = Path.Combine(Path.GetTempPath(), "officio-env-testes-" + Guid.NewGuid());

    public ExtensoesEnvTestes() => Directory.CreateDirectory(pasta);

    [Theory]
    [InlineData(false, false, false, "arquivo-env")]
    [InlineData(true, false, false, "user-secret")]
    [InlineData(true, true, false, "ambiente")]
    [InlineData(true, true, true, "argumento")]
    public void AdicionarArquivoEnv_PreservaPrecedenciaDosProviders(
        bool usarSecrets, bool usarAmbiente, bool usarArgumentos, string esperado)
    {
        File.WriteAllText(Path.Combine(pasta, "appsettings.json"), """{"Teste":{"Valor":"appsettings"}}""");
        File.WriteAllText(Path.Combine(pasta, "appsettings.Development.json"), """{"Teste":{"Valor":"appsettings-development"}}""");
        // User-secrets usa um provider JSON; este arquivo isolado evita ler/escrever segredos reais.
        File.WriteAllText(Path.Combine(pasta, "secrets.json"), """{"Teste":{"Valor":"user-secret"}}""");
        File.WriteAllText(Path.Combine(pasta, ".env"), "Teste__Valor=arquivo-env\nTeste__SomenteEnv=local");
        var prefixo = "OFFICIO_TESTE_" + Guid.NewGuid().ToString("N") + "_";
        try
        {
            var builder = new ConfigurationBuilder().SetBasePath(pasta)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json");
            if (usarSecrets) builder.AddJsonFile("secrets.json");
            if (usarAmbiente) Environment.SetEnvironmentVariable(prefixo + "Teste__Valor", "ambiente");
            builder.AddEnvironmentVariables(prefixo);
            builder.AddCommandLine(usarArgumentos ? ["--Teste:Valor=argumento"] : []);
            var fontesOriginais = builder.Sources.ToArray();

            builder.AdicionarArquivoEnv(Ambiente("Development"));
            using var config = (ConfigurationRoot)builder.Build();

            Assert.Equal(esperado, config["Teste:Valor"]);
            Assert.Equal("local", config["Teste:SomenteEnv"]);
            Assert.Equal(fontesOriginais, builder.Sources.Where(f => fontesOriginais.Contains(f)));
        }
        finally
        {
            Environment.SetEnvironmentVariable(prefixo + "Teste__Valor", null);
        }
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void AdicionarArquivoEnv_ForaDeDevelopmentNaoCarregaArquivo(string ambiente)
    {
        File.WriteAllText(Path.Combine(pasta, ".env"), "Teste__Valor=local");
        var builder = new ConfigurationBuilder();
        builder.AdicionarArquivoEnv(Ambiente(ambiente));
        using var config = (ConfigurationRoot)builder.Build();
        Assert.Empty(builder.Sources);
        Assert.Null(config["Teste:Valor"]);
    }

    [Fact]
    public void AdicionarArquivoEnv_ConfigurationManagerMantemArgumentosAcimaDoArquivo()
    {
        File.WriteAllText(Path.Combine(pasta, "appsettings.json"), """{"Teste":{"Valor":"appsettings"}}""");
        File.WriteAllText(Path.Combine(pasta, ".env"), "Teste__Valor=local\nTeste__SomenteEnv=local");
        using var config = new ConfigurationManager();
        config.SetBasePath(pasta);
        config.AddJsonFile("appsettings.json");
        config.AddCommandLine(["--Teste:Valor=argumento"]);

        config.AdicionarArquivoEnv(Ambiente("Development"));

        Assert.Equal("argumento", config["Teste:Valor"]);
        Assert.Equal("local", config["Teste:SomenteEnv"]);
    }

    private AmbienteTeste Ambiente(string nome) => new()
    {
        EnvironmentName = nome,
        ContentRootPath = pasta
    };

    public void Dispose() => Directory.Delete(pasta, recursive: true);

    private sealed class AmbienteTeste : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Officio.Testes";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
