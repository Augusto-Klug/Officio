using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Integracoes;

internal sealed class ConsultaMunicipioIbge(
    HttpClient httpClient,
    ILogger<ConsultaMunicipioIbge> logger) : IConsultaMunicipioIbge
{
    public async Task<IReadOnlyCollection<EstadoIbge>> ListarEstadosAsync(
        CancellationToken cancellationToken = default)
    {
        var estados = await ConsultarAsync<List<RespostaEstadoIbge>>(
            "ListarEstados",
            "api/v1/localidades/estados?orderBy=nome",
            cancellationToken);

        return estados?
            .Select(estado => new EstadoIbge(
                estado.Id.ToString(CultureInfo.InvariantCulture),
                estado.Sigla,
                estado.Nome))
            .ToList() ?? [];
    }

    public async Task<IReadOnlyCollection<MunicipioIbge>> ListarMunicipiosPorUfAsync(
        string uf,
        CancellationToken cancellationToken = default)
    {
        var ufNormalizada = uf.Trim().ToUpperInvariant();
        var municipios = await ObterMunicipiosAsync("ListarMunicipios", ufNormalizada, cancellationToken);

        return municipios
            .Select(municipio => new MunicipioIbge(
                municipio.Id.ToString(CultureInfo.InvariantCulture),
                municipio.Nome,
                ufNormalizada))
            .ToList();
    }

    public async Task<MunicipioIbge?> ObterPorNomeUfAsync(
        string nome,
        string uf,
        CancellationToken cancellationToken = default)
    {
        var ufNormalizada = uf.Trim().ToUpperInvariant();
        var municipios = await ObterMunicipiosAsync("ResolverMunicipio", ufNormalizada, cancellationToken);

        var municipio = municipios?.FirstOrDefault(item =>
            string.Equals(Normalizar(item.Nome), Normalizar(nome), StringComparison.Ordinal));

        return municipio is null
            ? null
            : new MunicipioIbge(
                municipio.Id.ToString(CultureInfo.InvariantCulture),
                municipio.Nome,
                ufNormalizada);
    }

    private async Task<IReadOnlyCollection<RespostaMunicipioIbge>> ObterMunicipiosAsync(
        string operacao,
        string uf,
        CancellationToken cancellationToken)
        => await ConsultarAsync<List<RespostaMunicipioIbge>>(
            operacao,
            $"api/v1/localidades/estados/{Uri.EscapeDataString(uf)}/municipios?orderBy=nome",
            cancellationToken) ?? [];

    private async Task<T?> ConsultarAsync<T>(string operacao, string caminho, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            var resultado = await httpClient.GetFromJsonAsync<T>(caminho, cancellationToken);
            var quantidade = resultado is System.Collections.ICollection colecao ? colecao.Count : (int?)null;
            logger.LogInformation(
                "Consulta IBGE concluida. Operacao={Operacao} Quantidade={Quantidade} DuracaoMs={DuracaoMs}",
                operacao,
                quantidade,
                timer.ElapsedMilliseconds);
            return resultado;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Falha na consulta IBGE. Operacao={Operacao} TipoExcecao={TipoExcecao} StatusHttp={StatusHttp} DuracaoMs={DuracaoMs} StackTrace={StackTrace}",
                operacao,
                exception.GetType().Name,
                (exception as HttpRequestException)?.StatusCode,
                timer.ElapsedMilliseconds,
                exception.StackTrace);
            throw;
        }
    }

    private static string Normalizar(string valor)
    {
        var decomposicao = valor.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var semAcentos = new string(decomposicao
            .Where(caractere => CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        return semAcentos.Normalize(NormalizationForm.FormC);
    }

    private sealed record RespostaEstadoIbge(long Id, string Sigla, string Nome);
    private sealed record RespostaMunicipioIbge(long Id, string Nome);
}
