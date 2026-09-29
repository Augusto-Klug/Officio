using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesBanco
{
    public static async Task InicializarBancoVazioAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();

        if (!contexto.Database.IsSqlServer())
            throw new InvalidOperationException("O bootstrap do schema exige SQL Server.");

        if (await contexto.Database.CanConnectAsync() && await PossuiTabelasDeUsuarioAsync(contexto))
            throw new InvalidOperationException(
                "O banco nao esta vazio. Use --apply-migrations para atualizar uma base existente.");

        if (!await contexto.Database.EnsureCreatedAsync())
            throw new InvalidOperationException(
                "O schema nao foi criado. Confirme que o banco esta vazio e tente novamente.");

        await RegistrarBaselineMigracoesAsync(contexto);
    }

    public static async Task AplicarMigrationsPendentesAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();

        if (!contexto.Database.IsSqlServer())
            throw new InvalidOperationException("A aplicacao de migrations exige SQL Server.");

        if (!await contexto.Database.CanConnectAsync() || !await PossuiTabelasDeUsuarioAsync(contexto))
            throw new InvalidOperationException(
                "A base nao possui schema inicial. Use --bootstrap-schema somente em um banco vazio.");

        await contexto.Database.MigrateAsync();
    }

    internal static async Task RegistrarBaselineMigracoesAsync(ContextoAplicacao contexto)
    {
        var repositorioHistorico = contexto.GetService<IHistoryRepository>();
        var assemblyMigracoes = contexto.GetService<IMigrationsAssembly>();

        await using var transacao = await contexto.Database.BeginTransactionAsync();
        await contexto.Database.ExecuteSqlRawAsync(repositorioHistorico.GetCreateIfNotExistsScript());

        foreach (var migrationId in assemblyMigracoes.Migrations.Keys.OrderBy(id => id))
        {
            var linha = new HistoryRow(migrationId, ProductInfo.GetVersion());
            await contexto.Database.ExecuteSqlRawAsync(repositorioHistorico.GetInsertScript(linha));
        }

        await transacao.CommitAsync();
    }

    private static async Task<bool> PossuiTabelasDeUsuarioAsync(ContextoAplicacao contexto)
    {
        var conexao = contexto.Database.GetDbConnection();
        var abriuConexao = conexao.State != System.Data.ConnectionState.Open;
        if (abriuConexao)
            await contexto.Database.OpenConnectionAsync();

        try
        {
            await using var comando = conexao.CreateCommand();
            comando.CommandText = "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0) THEN 1 ELSE 0 END";
            var resultado = await comando.ExecuteScalarAsync();
            return Convert.ToInt32(resultado) == 1;
        }
        finally
        {
            if (abriuConexao)
                await contexto.Database.CloseConnectionAsync();
        }
    }
}
