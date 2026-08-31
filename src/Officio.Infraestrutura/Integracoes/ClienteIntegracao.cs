using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Officio.Infraestrutura.Integracoes;

public class ClienteIntegracao<TConfiguracao>(
    HttpClient httpClient,
    IOptions<TConfiguracao> options) : IClienteIntegracao<TConfiguracao>
    where TConfiguracao : class, IConfiguracaoIntegracao
{
    public async Task<TResposta?> EnviarAsync<TResposta>(
        RequisicaoIntegracao<TResposta> requisicao,
        CancellationToken cancellationToken = default)
    {
        ConfigurarBaseAddress();

        using var httpRequest = new HttpRequestMessage(requisicao.Metodo, requisicao.Endpoint)
        {
            Content = requisicao.Conteudo
        };

        foreach (var header in requisicao.Headers)
            AplicarHeader(httpRequest, header.Key, header.Value);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<TResposta>(
            responseStream,
            RequisicaoBuilder.JsonOpcoes.Padrao,
            cancellationToken);
    }

    private void ConfigurarBaseAddress()
    {
        if (httpClient.BaseAddress is null && !string.IsNullOrWhiteSpace(options.Value.BaseUrl))
            httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    private static void AplicarHeader(HttpRequestMessage request, string nome, string valor)
    {
        if (nome.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
            && AuthenticationHeaderValue.TryParse(valor, out var authorization))
        {
            request.Headers.Authorization = authorization;
            return;
        }

        request.Headers.TryAddWithoutValidation(nome, valor);
    }
}
