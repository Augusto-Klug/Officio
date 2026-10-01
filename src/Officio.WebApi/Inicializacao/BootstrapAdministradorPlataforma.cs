using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Seguranca;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.WebApi.Inicializacao;

public static class BootstrapAdministradorPlataforma
{
    public static async Task ExecutarAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var cadastro = LerConfiguracao(configuration);
        var parceiroConfigurado = Parceiro.CriarDesenvolvimento(
            cadastro.TenantId,
            cadastro.Cnpj,
            cadastro.NomeParceiro,
            cadastro.NomeFantasiaParceiro,
            cadastro.EmailParceiro);

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var parceiroExistente = await db.Parceiros.IgnoreQueryFilters()
            .SingleOrDefaultAsync(parceiro => parceiro.Id == cadastro.TenantId, cancellationToken);

        if (parceiroExistente is not null &&
            (parceiroExistente.Cnpj != parceiroConfigurado.Cnpj ||
             parceiroExistente.RazaoSocial != parceiroConfigurado.RazaoSocial))
            throw new InvalidOperationException(
                "O parceiro informado ja existe com CNPJ ou razao social diferentes; nenhum cadastro foi alterado.");

        if (parceiroExistente is null && await db.Parceiros.IgnoreQueryFilters()
                .AnyAsync(parceiro => parceiro.Cnpj == parceiroConfigurado.Cnpj, cancellationToken))
            throw new InvalidOperationException(
                "O CNPJ informado ja pertence a outro parceiro; nenhum cadastro foi criado.");

        var usuarioExistente = await db.Users.IgnoreQueryFilters()
            .SingleOrDefaultAsync(usuario => usuario.Email == cadastro.Email, cancellationToken);

        if (usuarioExistente is not null)
        {
            if (usuarioExistente.TenantId != cadastro.TenantId ||
                usuarioExistente.Role != PerfisAcesso.PlatformAdmin ||
                !usuarioExistente.Ativo || parceiroExistente is null)
                throw new InvalidOperationException(
                    "O e-mail ja esta associado a outro usuario ou a um cadastro inconsistente; nenhum dado foi alterado.");

            return;
        }

        var usuario = Usuario.Create(cadastro.TenantId, cadastro.Nome, cadastro.Email, string.Empty,
            PerfisAcesso.PlatformAdmin);
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, cadastro.Senha));

        if (parceiroExistente is null)
            db.Parceiros.Add(parceiroConfigurado);
        db.Users.Add(usuario);

        await db.SaveChangesAsync(cancellationToken);
    }

    private static ConfiguracaoBootstrap LerConfiguracao(IConfiguration configuration)
    {
        var tenant = configuration["BootstrapUser:TenantId"];
        var nomeParceiro = configuration["BootstrapUser:TenantNome"]?.Trim();
        var cnpj = configuration["BootstrapUser:TenantCnpj"];
        var nomeFantasia = configuration["BootstrapUser:TenantNomeFantasia"];
        var emailParceiro = configuration["BootstrapUser:TenantEmail"];
        var nome = configuration["BootstrapUser:Nome"]?.Trim();
        var email = configuration["BootstrapUser:Email"]?.Trim();
        var senha = configuration["BootstrapUser:Password"];
        var role = configuration["BootstrapUser:Role"]?.Trim();

        if (!configuration.GetValue<bool>("BootstrapUser:Enabled"))
            throw new InvalidOperationException(
                "Habilite BootstrapUser:Enabled somente durante a execucao explicita do bootstrap.");

        if (!Guid.TryParse(tenant, out var tenantId) || tenantId == Guid.Empty ||
            string.IsNullOrWhiteSpace(nomeParceiro) || string.IsNullOrWhiteSpace(cnpj) ||
            string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(senha))
            throw new InvalidOperationException(
                "Informe TenantId, TenantNome, TenantCnpj, Nome, Email e Password para o bootstrap.");

        if (!string.Equals(role, PerfisAcesso.PlatformAdmin, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "BootstrapUser:Role deve ser PlatformAdmin para este comando.");

        return new ConfiguracaoBootstrap(
            tenantId,
            nomeParceiro,
            cnpj,
            nomeFantasia,
            emailParceiro,
            nome,
            email,
            senha);
    }

    private sealed record ConfiguracaoBootstrap(
        Guid TenantId,
        string NomeParceiro,
        string Cnpj,
        string? NomeFantasiaParceiro,
        string? EmailParceiro,
        string Nome,
        string Email,
        string Senha);
}
