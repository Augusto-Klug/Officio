using System.Collections.Immutable;
using System.Text.Json;

namespace Officio.Infraestrutura.Pdf.Conteudo;

internal sealed record AberturaContrato(
    string Titulo, string Contratante, string Contratada, string Introducao, string Fechamento);
internal sealed record BlocoContrato(string Tipo, string Texto);
internal sealed record ClausulaContrato(string Titulo, ImmutableArray<BlocoContrato> Blocos);
internal sealed record AreaAnexo(string Titulo, ImmutableArray<string> Itens);
internal sealed record AnexoContrato(string Titulo, ImmutableArray<AreaAnexo> Areas);

internal static class ConteudoContrato
{
    private static readonly JsonSerializerOptions Opcoes = new() { PropertyNameCaseInsensitive = true };
    private static readonly Lazy<AberturaContrato> AberturaRecurso = new(() => Ler<AberturaContrato>("abertura"));
    private static readonly Lazy<ImmutableArray<ClausulaContrato>> ClausulasRecurso = new(() =>
        Enumerable.Range(1, 7).Select(i => Ler<ClausulaContrato>($"clausula-{i}")).ToImmutableArray());
    private static readonly Lazy<AnexoContrato> AnexoRecurso = new(() => Ler<AnexoContrato>("anexo"));

    public static AberturaContrato Abertura => AberturaRecurso.Value;
    public static ImmutableArray<ClausulaContrato> Clausulas => ClausulasRecurso.Value;
    public static AnexoContrato Anexo => AnexoRecurso.Value;

    private static T Ler<T>(string nome)
    {
        using var recurso = typeof(ConteudoContrato).Assembly.GetManifestResourceStream(
            $"Officio.Infraestrutura.Pdf.Conteudo.{nome}.json")
            ?? throw new InvalidOperationException($"Recurso contratual ausente: {nome}.");
        return JsonSerializer.Deserialize<T>(recurso, Opcoes)
            ?? throw new InvalidOperationException($"Recurso contratual inválido: {nome}.");
    }
}
