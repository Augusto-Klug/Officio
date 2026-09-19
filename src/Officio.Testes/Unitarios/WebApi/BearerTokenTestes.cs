using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;
using Officio.WebApi.Extensoes;
using Officio.WebApi.Intermediarios;

namespace Officio.Testes.Unitarios.WebApi;

public class BearerTokenTestes
{
    [Theory]
    [InlineData(false, 401)]
    [InlineData(true, 200)]
    public async Task Pipeline_EndpointAuthorize_ExigeBearerValido(bool enviarToken, int status)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "chave-exclusiva-para-testes-bearer-officio",
            ["Jwt:Issuer"] = "Officio.Testes",
            ["Jwt:Audience"] = "Officio.Testes"
        }).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddRouting();
        services.AdicionarServicosOfficio(config);
        services.AddScoped<IVerificadorAcesso, VerificadorAcessoFake>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var usuario = Usuario.Create(Guid.NewGuid(), "Maria", "maria@test.local", "", "Admin");
        usuario.UpdatePassword(new PasswordHasher<Usuario>().HashPassword(usuario, "senha"));
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, new ServicoEmpresaAtual(), new PublicadorEventosFake());
        db.Users.Add(usuario);
        await db.SaveChangesAsync();
        var login = await new ServicoJwt(config, db).AutenticarAsync(usuario.Email, "senha");

        var app = new ApplicationBuilder(provider);
        app.UseAuthentication();
        app.UseMiddleware<IntermediarioEmpresa>();
        app.UseAuthorization();
        var executado = false;
        app.Run(context =>
        {
            executado = true;
            Assert.True(context.User.Identity!.IsAuthenticated);
            Assert.True(context.User.IsInRole("Admin"));
            Assert.Equal(usuario.TenantId, context.RequestServices.GetRequiredService<IEmpresaAtual>().TenantId);
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Path = "/api/teste-protegido";
        context.Response.Body = new MemoryStream();
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
            new EndpointMetadataCollection(new AuthorizeAttribute()), "Protegido"));
        if (enviarToken)
            context.Request.Headers.Authorization = "Bearer " + login!.Token;
        await app.Build()(context);
        Assert.Equal(status, context.Response.StatusCode);
        Assert.Equal(enviarToken, executado);
    }

    private sealed class VerificadorAcessoFake : IVerificadorAcesso
    {
        public Task<bool> UsuarioAtivoAsync(Guid usuarioId, CancellationToken ct = default)
            => Task.FromResult(true);

        public Task<bool> EmpresaAtivaAsync(Guid empresaId, CancellationToken ct = default)
            => Task.FromResult(true);
    }
}
