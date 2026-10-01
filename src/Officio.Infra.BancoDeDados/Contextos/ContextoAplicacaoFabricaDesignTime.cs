using Microsoft.Data.SqlClient;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Officio.Dominio.Interfaces;
using System.Text.Json;

namespace Officio.Infra.BancoDeDados.Contextos;

public sealed class ContextoAplicacaoFabricaDesignTime : IDesignTimeDbContextFactory<ContextoAplicacao>
{
    public ContextoAplicacao CreateDbContext(string[] args)
    {
        CarregarArquivoEnv();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? ObterConnectionStringAppsettings()
            ?? throw new InvalidOperationException(
                "Configure ConnectionStrings__DefaultConnection no .env ou DefaultConnection no appsettings.Development.json.");

        var senha = Environment.GetEnvironmentVariable("Database__Password");
        if (!string.IsNullOrWhiteSpace(senha))
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.Password))
                builder.Password = senha;

            connectionString = builder.ConnectionString;
        }

        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseSqlServer(connectionString, configuracao =>
                configuracao
                    .MigrationsAssembly(typeof(ContextoAplicacao).Assembly.FullName)
                    .EnableRetryOnFailure())
            .Options;

        return new ContextoAplicacao(options, new EmpresaAtualDesignTime(), new PublicadorEventosNulo());
    }

    private static void CarregarArquivoEnv()
    {
        var diretorio = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (diretorio is not null)
        {
            var caminho = Path.Combine(diretorio.FullName, ".env");
            if (File.Exists(caminho))
            {
                foreach (var linha in File.ReadLines(caminho))
                {
                    var variavel = LerLinha(linha);
                    if (variavel is null)
                        continue;

                    Environment.SetEnvironmentVariable(variavel.Value.Chave, variavel.Value.Valor);
                }

                return;
            }

            diretorio = diretorio.Parent;
        }
    }

    private static string? ObterConnectionStringAppsettings()
    {
        var diretorio = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (diretorio is not null)
        {
            var candidatos = new[]
            {
                Path.Combine(diretorio.FullName, "Officio.WebApi", "appsettings.Development.json"),
                Path.Combine(diretorio.FullName, "src", "Officio.WebApi", "appsettings.Development.json")
            };

            var caminho = candidatos.FirstOrDefault(File.Exists);
            if (caminho is not null)
            {
                using var documento = JsonDocument.Parse(File.ReadAllText(caminho));
                if (documento.RootElement.TryGetProperty("ConnectionStrings", out var conexoes) &&
                    conexoes.TryGetProperty("DefaultConnection", out var valor))
                    return valor.GetString();
            }

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

    private sealed class EmpresaAtualDesignTime : IEmpresaAtual
    {
        public Guid TenantId => Guid.Empty;

        public void SetTenant(Guid tenantId) { }
    }

    private sealed class PublicadorEventosNulo : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
