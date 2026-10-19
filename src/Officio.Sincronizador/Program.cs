using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EncerrarContratosExpirados;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ReconciliarAssinaturasAutentique;
using Officio.Dominio.Mediador.Modulos;
using Officio.Infra.BancoDeDados.Modulos;
using Officio.Infraestrutura.Modulos;
using Officio.Sincronizador.Configuracao;
using Officio.Sincronizador.TempoReal;
using Officio.Dominio.Interfaces;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AdicionarArquivoEnv(builder.Environment);
builder.Services.AddHttpContextAccessor();
builder.Services.AdicionarMediadores(builder.Configuration);
builder.Services.AdicionarBancoDeDados(builder.Configuration);
builder.Services.AdicionarInfraestrutura(builder.Configuration);
builder.Services.AddHttpClient<IPublicadorAtualizacaoTempoReal, PublicadorAtualizacaoTempoRealHttp>(
    cliente => cliente.Timeout = TimeSpan.FromSeconds(5));

using var host = builder.Build();
var fusoBrasil = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
var hojeNoBrasil = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, fusoBrasil).DateTime);
var reconciliacaoConcluida = false;
var encerramentoConcluido = false;

try
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var reconciliacao = await mediator.Send(new ReconciliarAssinaturasAutentiqueComando());
    reconciliacaoConcluida = reconciliacao.Sucesso && reconciliacao.Valor!.Falhas == 0;

    if (!reconciliacao.Sucesso)
        Console.Error.WriteLine($"Falha na reconciliacao Autentique: {reconciliacao.Erro}");
    else
    {
        var resumo = reconciliacao.Valor!;
        Console.WriteLine(
            $"Reconciliacao concluida. Consultadas={resumo.Consultadas} Concluidas={resumo.Concluidas} " +
            $"Pendentes={resumo.Pendentes} Falhas={resumo.Falhas}");
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Excecao na reconciliacao Autentique: {exception.Message}");
}

try
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var encerramento = await mediator.Send(new EncerrarContratosExpiradosComando(hojeNoBrasil));
    encerramentoConcluido = encerramento.Sucesso;

    if (!encerramento.Sucesso)
        Console.Error.WriteLine($"Falha ao encerrar contratos expirados: {encerramento.Erro}");
    else
        Console.WriteLine($"Encerramento de contratos concluido. DataReferencia={hojeNoBrasil} Encerrados={encerramento.Valor}");
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Excecao ao encerrar contratos expirados: {exception.Message}");
}

return reconciliacaoConcluida && encerramentoConcluido ? 0 : 2;
