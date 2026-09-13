using Officio.WebApi.Extensoes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AdicionarServicosOfficio(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await Officio.WebApi.Desenvolvimento.BootstrapUsuario.ExecutarAsync(app.Services, app.Configuration, app.Environment);
}

app.UseHttpsRedirection();
app.UsarMiddlewaresOfficio();

app.MapControllers();

app.Run();




