using Officio.WebApi.Extensoes;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AdicionarArquivoEnv(builder.Environment);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AdicionarOpenApiOfficio();
builder.Services.AdicionarServicosOfficio(builder.Configuration);

var app = builder.Build();

await app.PrepararBancoDesenvolvimentoAsync();

app.UsarSwaggerOfficio();

app.UseHttpsRedirection();
app.UsarMiddlewaresOfficio();

app.MapControllers();

app.Run();

public partial class Program;




