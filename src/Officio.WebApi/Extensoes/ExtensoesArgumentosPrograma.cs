using Microsoft.AspNetCore.Builder;
using Officio.WebApi.Inicializacao;

namespace Officio.WebApi.Extensoes;

public sealed record ArgumentosPrograma(string[] ArgumentosHost, string? Comando);

public static class ExtensoesArgumentosPrograma
{
    private const string ComandoBootstrapSchema = "--bootstrap-schema";
    private const string ComandoAplicarMigrations = "--apply-migrations";
    private const string ComandoBootstrapAdministradorPlataforma = "--bootstrap-platform-admin";

    private static readonly string[] ComandosPrograma =
    [
        ComandoBootstrapSchema,
        ComandoAplicarMigrations,
        ComandoBootstrapAdministradorPlataforma
    ];

    public static ArgumentosPrograma AnalisarArgumentosPrograma(this string[] argumentos)
    {
        var comandos = argumentos.Where(argumento =>
            ComandosPrograma.Contains(argumento, StringComparer.OrdinalIgnoreCase)).ToArray();

        if (comandos.Length > 1)
            throw new ArgumentException("Escolha apenas um comando operacional por execucao.");

        var argumentosHost = argumentos.Except(comandos, StringComparer.OrdinalIgnoreCase).ToArray();
        return new ArgumentosPrograma(argumentosHost, comandos.SingleOrDefault());
    }

    public static async Task<bool> ExecutarComandoProgramaAsync(
        this WebApplication app,
        string? comando)
    {
        if (comando is null)
            return false;

        try
        {
            switch (comando)
            {
                case ComandoBootstrapSchema:
                    await app.Services.InicializarBancoVazioAsync();
                    Console.WriteLine("Schema base e baseline de migrations criados.");
                    break;
                case ComandoAplicarMigrations:
                    await app.Services.AplicarMigrationsPendentesAsync();
                    Console.WriteLine("Migrations pendentes aplicadas.");
                    break;
                case ComandoBootstrapAdministradorPlataforma:
                    await BootstrapAdministradorPlataforma.ExecutarAsync(
                        app.Services, app.Configuration);
                    Console.WriteLine("Bootstrap do PlatformAdmin concluido ou ja existente.");
                    break;
                default:
                    throw new ArgumentException($"Comando operacional desconhecido: {comando}.");
            }
        }
        finally
        {
            await app.DisposeAsync();
        }

        return true;
    }
}
