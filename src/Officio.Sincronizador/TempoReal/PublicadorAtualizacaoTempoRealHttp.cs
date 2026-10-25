using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Sincronizador.TempoReal;

public sealed class PublicadorAtualizacaoTempoRealHttp(
    HttpClient cliente,
    IConfiguration configuracao,
    ILogger<PublicadorAtualizacaoTempoRealHttp> logger) : IPublicadorAtualizacaoTempoReal
{
    public const string HeaderChave = "X-Officio-Internal-Key";
    public const string RotaCallback = "/api/v1/interno/atualizacoes-tempo-real";

    public async Task PublicarAsync(
        EventoAtualizacaoTempoReal evento,
        CancellationToken cancellationToken = default)
    {
        var url = configuracao["TempoReal:CallbackUrl"];
        var chave = configuracao["TempoReal:ChaveCompartilhada"];
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate($"{url.TrimEnd('/')}{RotaCallback}", UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrWhiteSpace(chave))
        {
            logger.LogWarning("Publicacao SignalR ignorada: configure TempoReal:CallbackUrl e TempoReal:ChaveCompartilhada.");
            return;
        }

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(evento)
        };
        requisicao.Headers.TryAddWithoutValidation(HeaderChave, chave);

        try
        {
            using var resposta = await cliente.SendAsync(requisicao, cancellationToken);
            if (!resposta.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Falha ao publicar atualizacao SignalR via callback. HttpStatus={HttpStatus} ParceiroId={ParceiroId} Entidade={Entidade} RegistroId={RegistroId}",
                    (int)resposta.StatusCode,
                    evento.ParceiroId,
                    evento.Entidade,
                    evento.Id);
                return;
            }

            logger.LogInformation(
                "Callback SignalR aceitou a atualizacao. ParceiroId={ParceiroId} Entidade={Entidade} RegistroId={RegistroId}",
                evento.ParceiroId,
                evento.Entidade,
                evento.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Falha best-effort ao publicar atualizacao SignalR via callback. ParceiroId={ParceiroId} Entidade={Entidade} RegistroId={RegistroId} ErroTipo={ErroTipo} StackTrace={StackTrace}",
                evento.ParceiroId,
                evento.Entidade,
                evento.Id,
                exception.GetType().FullName,
                exception.StackTrace);
        }
    }
}
