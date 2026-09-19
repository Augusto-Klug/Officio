using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using System.Data;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesBancoDesenvolvimento
{
    public static async Task<WebApplication> PrepararBancoDesenvolvimentoAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() ||
            !app.Configuration.GetValue<bool>("Database:EnsureCreatedOnStartup"))
            return app;

        using var scope = app.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();

        var schemaCriado = await contexto.Database.EnsureCreatedAsync();

        if (!schemaCriado && !await TabelaUsuariosExisteAsync(contexto))
            throw new InvalidOperationException(
                "O banco configurado existe, mas nao contem o schema base do Officio. " +
                "Para teste local, remova esse banco ou use outro nome em ConnectionStrings:DefaultConnection e suba a API novamente. " +
                "Nao execute as migrations incrementais em uma base vazia.");

        await CriarUsuarioAdministradorAsync(contexto, app.Configuration);

        return app;
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

    private static async Task CriarUsuarioAdministradorAsync(
        ContextoAplicacao contexto,
        IConfiguration configuration)
    {
        var seed = ObterSeedAdministrador(configuration);
        if (seed is null)
            return;

        var adminExiste = await contexto.Users
            .IgnoreQueryFilters()
            .AnyAsync(usuario => usuario.Email == seed.Email);

        if (adminExiste)
            return;

        var empresa = Empresa.Create(seed.EmpresaNome, seed.EmpresaDocumento);
        var usuario = Usuario.Create(empresa.Id, seed.Nome, seed.Email, string.Empty, seed.Perfil);
        var senhaHash = new PasswordHasher<Usuario>().HashPassword(usuario, seed.Senha);
        usuario.UpdatePassword(senhaHash);

        contexto.Empresas.Add(empresa);
        contexto.Users.Add(usuario);

        await contexto.SaveChangesAsync();
    }

    private static SeedAdministrador? ObterSeedAdministrador(IConfiguration configuration)
    {
        var email = configuration["DevelopmentSeed:Admin:Email"];
        var senha = configuration["DevelopmentSeed:Admin:Password"];
        var nome = configuration["DevelopmentSeed:Admin:Name"];
        var perfil = configuration["DevelopmentSeed:Admin:Role"];
        var empresaNome = configuration["DevelopmentSeed:Company:Name"];
        var empresaDocumento = configuration["DevelopmentSeed:Company:Document"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(senha) ||
            string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(perfil) ||
            string.IsNullOrWhiteSpace(empresaNome) ||
            string.IsNullOrWhiteSpace(empresaDocumento))
            return null;

        return new SeedAdministrador(email, senha, nome, perfil, empresaNome, empresaDocumento);
    }

    private sealed record SeedAdministrador(
        string Email,
        string Senha,
        string Nome,
        string Perfil,
        string EmpresaNome,
        string EmpresaDocumento);
}
