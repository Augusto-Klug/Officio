using Officio.WebApi.Extensoes;
using Officio.WebApi.Saude;
using Officio.WebApi.TempoReal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var argumentosPrograma = args.AnalisarArgumentosPrograma();
var builder = WebApplication.CreateBuilder(argumentosPrograma.ArgumentosHost);

builder.Configuration.AdicionarArquivoEnv(builder.Environment);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddCheck<VerificacaoBancoDados>("banco", tags: ["ready"]);
builder.Services.AdicionarCorsFrontend(builder.Configuration);
builder.Services.AdicionarOpenApiOfficio();
builder.Services.AdicionarObservabilidade(builder.Configuration);
builder.Services.AdicionarServicosOfficio(builder.Configuration);

var app = builder.Build();

if (await app.ExecutarComandoProgramaAsync(argumentosPrograma.Comando))
    return;

await app.PrepararBancoDesenvolvimentoAsync();

app.UsarSwaggerOfficio();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UsarMiddlewaresOfficio();

app.MapControllers();
app.MapHub<AtualizacoesHub>("/api/v1/tempo-real");
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

public partial class Program;



