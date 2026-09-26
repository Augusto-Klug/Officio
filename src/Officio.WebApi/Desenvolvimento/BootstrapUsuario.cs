using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.WebApi.Desenvolvimento;

// Fluxo de criacao do usuario local, chamado por PrepararBancoDesenvolvimentoAsync.
public static class BootstrapUsuario
{
    public static async Task ExecutarAsync(IServiceProvider services, IConfiguration configuration,
        IHostEnvironment environment, CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment() || !configuration.GetValue<bool>("BootstrapUser:Enabled"))
            return;

        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BootstrapUsuario");
        var tenant = configuration["BootstrapUser:TenantId"];
        var nome = configuration["BootstrapUser:Nome"]?.Trim();
        var nomeParceiro = configuration["BootstrapUser:TenantNome"]?.Trim();
        var cnpjParceiro = configuration["BootstrapUser:TenantCnpj"];
        var nomeFantasiaParceiro = configuration["BootstrapUser:TenantNomeFantasia"];
        var emailParceiro = configuration["BootstrapUser:TenantEmail"];
        var email = configuration["BootstrapUser:Email"]?.Trim();
        var password = configuration["BootstrapUser:Password"];
        var role = configuration["BootstrapUser:Role"]?.Trim();
        if (!Guid.TryParse(tenant, out var tenantId) || tenantId == Guid.Empty ||
            string.IsNullOrWhiteSpace(nomeParceiro) || string.IsNullOrWhiteSpace(cnpjParceiro) ||
            string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
        {
            logger.LogWarning(
                "BootstrapUser ignorado: informe TenantId valido, TenantNome, TenantCnpj, Nome, Email, Password e Role na configuracao.");
            return;
        }

        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        // Ainda nao existe requisicao autenticada; a unicidade do email e global.
        var existente = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (existente is not null)
        {
            if (existente.TenantId != tenantId)
                logger.LogWarning("BootstrapUser ignorado: email ja cadastrado em outro parceiro.");
            else
                logger.LogInformation("BootstrapUser: usuario ja existe; o parceiro sera verificado.");
            if (existente.TenantId != tenantId)
                return;
        }

        var parceiro = await db.Parceiros.FindAsync([tenantId], cancellationToken);
        if (parceiro is null)
        {
            parceiro = Parceiro.CriarDesenvolvimento(
                tenantId,
                cnpjParceiro,
                nomeParceiro,
                nomeFantasiaParceiro,
                emailParceiro);
            db.Parceiros.Add(parceiro);
        }
        else
        {
            parceiro.AtualizarCadastro(cnpjParceiro, nomeParceiro, nomeFantasiaParceiro, emailParceiro);
        }

        if (existente is null)
        {
            var usuario = Usuario.Create(tenantId, nome, email, string.Empty, role);
            usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, password));
            db.Users.Add(usuario);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "BootstrapUser: parceiro e usuario de desenvolvimento preparados para {TenantId}.",
            tenantId);
    }
}
