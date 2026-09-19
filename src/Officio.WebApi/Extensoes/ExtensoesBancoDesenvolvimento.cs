using Microsoft.EntityFrameworkCore;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.WebApi.Desenvolvimento;
using System.Data;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesBancoDesenvolvimento
{
    public static async Task<WebApplication> PrepararBancoDesenvolvimentoAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        if (app.Configuration.GetValue<bool>("Database:EnsureCreatedOnStartup"))
            await PrepararSchemaAsync(app.Services);

        await BootstrapUsuario.ExecutarAsync(app.Services, app.Configuration, app.Environment);
        return app;
    }

    private static async Task PrepararSchemaAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();

        var schemaCriado = await contexto.Database.EnsureCreatedAsync();

        if (!schemaCriado && !await TabelaUsuariosExisteAsync(contexto))
            throw new InvalidOperationException(
                "O banco configurado existe, mas nao contem o schema base do Officio. " +
                "Para teste local, remova esse banco ou use outro nome em ConnectionStrings:DefaultConnection e suba a API novamente. " +
                "Nao execute as migrations incrementais em uma base vazia.");

    }

    private static async Task<bool> TabelaUsuariosExisteAsync(ContextoAplicacao contexto)
    {
        var conexao = contexto.Database.GetDbConnection();
        var abrirConexao = conexao.State != ConnectionState.Open;

        if (abrirConexao)
            await conexao.OpenAsync();

        try
        {
            await using var comando = conexao.CreateCommand();
            comando.CommandText = "SELECT CASE WHEN OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL THEN 0 ELSE 1 END";

            var resultado = await comando.ExecuteScalarAsync();
            return Convert.ToInt32(resultado) == 1;
        }
        finally
        {
            if (abrirConexao)
                await conexao.CloseAsync();
        }
    }

}
