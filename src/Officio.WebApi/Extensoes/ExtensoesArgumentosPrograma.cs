using Microsoft.AspNetCore.Builder;

namespace Officio.WebApi.Extensoes;

public sealed record ArgumentosPrograma(string[] ArgumentosHost, string? ComandoBanco);

public static class ExtensoesArgumentosPrograma
{
    private const string ComandoBootstrapSchema = "--bootstrap-schema";
    private const string ComandoAplicarMigrations = "--apply-migrations";

    public static ArgumentosPrograma AnalisarArgumentosPrograma(this string[] argumentos)
    {
        var comandosBanco = argumentos.Where(argumento =>
            argumento.Equals(ComandoBootstrapSchema, StringComparison.OrdinalIgnoreCase) ||
            argumento.Equals(ComandoAplicarMigrations, StringComparison.OrdinalIgnoreCase)).ToArray();

        if (comandosBanco.Length > 1)
            throw new ArgumentException("Escolha apenas um comando de banco por execucao.");

        var argumentosHost = argumentos.Except(comandosBanco, StringComparer.OrdinalIgnoreCase).ToArray();
        return new ArgumentosPrograma(argumentosHost, comandosBanco.SingleOrDefault());
    }

    public static async Task<bool> ExecutarComandoBancoAsync(
        this WebApplication app,
        string? comandoBanco)
    {
        if (comandoBanco is null)
            return false;

        try
        {
            switch (comandoBanco)
            {
                case ComandoBootstrapSchema:
                    await app.Services.InicializarBancoVazioAsync();
                    Console.WriteLine("Schema base e baseline de migrations criados.");
                    break;
                case ComandoAplicarMigrations:
                    await app.Services.AplicarMigrationsPendentesAsync();
                    Console.WriteLine("Migrations pendentes aplicadas.");
                    break;
                default:
                    throw new ArgumentException($"Comando de banco desconhecido: {comandoBanco}.");
            }
        }
        finally
        {
            await app.DisposeAsync();
        }

        return true;
    }
}
