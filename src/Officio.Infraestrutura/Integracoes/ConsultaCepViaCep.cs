using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Integracoes;

internal sealed class ConsultaCepViaCep(
    HttpClient httpClient,
    TimeProvider timeProvider,
    ILogger<ConsultaCepViaCep> logger) : IConsultaCep
{
    private static readonly ConcurrentDictionary<string, ItemCache> Cache = new();
    private static readonly TimeSpan DuracaoCache = TimeSpan.FromHours(24);

    public async Task<EnderecoCep?> ConsultarAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        if (Cache.TryGetValue(cep, out var item) && item.ExpiraEm > timeProvider.GetUtcNow())
            return item.Endereco;

        var timer = Stopwatch.StartNew();
        RespostaViaCep? resposta;
        try
        {
            resposta = await httpClient.GetFromJsonAsync<RespostaViaCep>(
                $"ws/{Uri.EscapeDataString(cep)}/json/",
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Falha na consulta ViaCEP. TipoExcecao={TipoExcecao} StatusHttp={StatusHttp} DuracaoMs={DuracaoMs} StackTrace={StackTrace}",
                exception.GetType().Name,
                (exception as HttpRequestException)?.StatusCode,
                timer.ElapsedMilliseconds,
                exception.StackTrace);
            throw;
        }

        if (resposta is null || ErroSinalizado(resposta.Erro) ||
            string.IsNullOrWhiteSpace(resposta.Localidade) ||
            string.IsNullOrWhiteSpace(resposta.Uf) ||
            string.IsNullOrWhiteSpace(resposta.Ibge))
        {
            logger.LogInformation("Consulta ViaCEP concluida sem endereco. DuracaoMs={DuracaoMs}", timer.ElapsedMilliseconds);
            return null;
        }

        var endereco = new EnderecoCep(
            SomenteDigitos(resposta.Cep) ?? cep,
            Normalizar(resposta.Logradouro),
            Normalizar(resposta.Complemento),
            Normalizar(resposta.Bairro),
            resposta.Localidade.Trim(),
            resposta.Uf.Trim().ToUpperInvariant(),
            resposta.Ibge.Trim());

        Cache[cep] = new ItemCache(endereco, timeProvider.GetUtcNow().Add(DuracaoCache));
        logger.LogInformation("Consulta ViaCEP concluida com endereco. DuracaoMs={DuracaoMs}", timer.ElapsedMilliseconds);
        return endereco;
    }

    private static string? Normalizar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static bool ErroSinalizado(JsonElement erro)
        => erro.ValueKind == JsonValueKind.True ||
           erro.ValueKind == JsonValueKind.String &&
           bool.TryParse(erro.GetString(), out var valor) && valor;

    private static string? SomenteDigitos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return new string(valor.Where(char.IsDigit).ToArray());
    }

    private sealed record ItemCache(EnderecoCep Endereco, DateTimeOffset ExpiraEm);

    private sealed record RespostaViaCep(
        string? Cep,
        string? Logradouro,
        string? Complemento,
        string? Bairro,
        string? Localidade,
        string? Uf,
        string? Ibge,
        [property: JsonPropertyName("erro")] JsonElement Erro = default);
}
