using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ReconciliarAssinaturasAutentique;
using Officio.Dominio.Mediador.Modulos;
using Officio.Infra.BancoDeDados.Modulos;
using Officio.Infraestrutura.Modulos;
using Officio.Sincronizador.Configuracao;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AdicionarArquivoEnv(builder.Environment);
builder.Services.AddHttpContextAccessor();
builder.Services.AdicionarMediadores(builder.Configuration);
builder.Services.AdicionarBancoDeDados(builder.Configuration);
builder.Services.AdicionarInfraestrutura(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
var resultado = await mediator.Send(new ReconciliarAssinaturasAutentiqueComando());

if (!resultado.Sucesso)
{
    Console.Error.WriteLine($"Falha na reconciliacao Autentique: {resultado.Erro}");
    return 1;
}

var resumo = resultado.Valor!;
Console.WriteLine(
    $"Reconciliacao concluida. Consultadas={resumo.Consultadas} Concluidas={resumo.Concluidas} " +
    $"Pendentes={resumo.Pendentes} Falhas={resumo.Falhas}");

return resumo.Falhas == 0 ? 0 : 2;
