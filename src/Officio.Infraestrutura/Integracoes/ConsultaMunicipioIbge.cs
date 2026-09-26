using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Integracoes;

internal sealed class ConsultaMunicipioIbge(HttpClient httpClient) : IConsultaMunicipioIbge
{
    public async Task<IReadOnlyCollection<EstadoIbge>> ListarEstadosAsync(
        CancellationToken cancellationToken = default)
    {
        var estados = await httpClient.GetFromJsonAsync<List<RespostaEstadoIbge>>(
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
        var municipios = await ObterMunicipiosAsync(ufNormalizada, cancellationToken);

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
        var municipios = await ObterMunicipiosAsync(ufNormalizada, cancellationToken);

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
        string uf,
        CancellationToken cancellationToken)
        => await httpClient.GetFromJsonAsync<List<RespostaMunicipioIbge>>(
            $"api/v1/localidades/estados/{Uri.EscapeDataString(uf)}/municipios?orderBy=nome",
            cancellationToken) ?? [];

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
