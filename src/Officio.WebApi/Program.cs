using Officio.WebApi.Extensoes;

var argumentosPrograma = args.AnalisarArgumentosPrograma();
var builder = WebApplication.CreateBuilder(argumentosPrograma.ArgumentosHost);

builder.Configuration.AdicionarArquivoEnv(builder.Environment);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AdicionarCorsFrontend(builder.Configuration);
builder.Services.AdicionarOpenApiOfficio();
builder.Services.AdicionarObservabilidade(builder.Configuration);
builder.Services.AdicionarServicosOfficio(builder.Configuration);

var app = builder.Build();

if (await app.ExecutarComandoBancoAsync(argumentosPrograma.ComandoBanco))
    return;

await app.PrepararBancoDesenvolvimentoAsync();

app.UsarSwaggerOfficio();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UsarMiddlewaresOfficio();

app.MapControllers();

app.Run();

public partial class Program;




