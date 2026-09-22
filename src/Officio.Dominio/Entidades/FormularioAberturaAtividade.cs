using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class FormularioAberturaAtividade : EntidadeBase
{
    public Guid FormularioAberturaId { get; private set; }
    public string DescricaoAtividade { get; private set; } = default!;
    public string? Cnae { get; private set; }
    public decimal? Percentual { get; private set; }
    public bool? RealizaNoEndereco { get; private set; }
    public int? Ordem { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;
    public FormularioAbertura FormularioAbertura { get; private set; } = default!;

    private FormularioAberturaAtividade() { }

    public static FormularioAberturaAtividade Criar(
        Guid formularioAberturaId,
        string descricaoAtividade,
        string? cnae,
        decimal? percentual,
        bool? realizaNoEndereco,
        int? ordem)
    {
        if (string.IsNullOrWhiteSpace(descricaoAtividade))
            throw new ArgumentException("Descricao da atividade e obrigatoria.", nameof(descricaoAtividade));
        if (percentual is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentual), "Percentual da atividade deve estar entre 0 e 100.");
        if (ordem is < 0)
            throw new ArgumentOutOfRangeException(nameof(ordem), "Ordem da atividade nao pode ser negativa.");

        return new()
        {
            FormularioAberturaId = formularioAberturaId,
            DescricaoAtividade = descricaoAtividade.Trim(),
            Cnae = NormalizarCnae(cnae),
            Percentual = percentual,
            RealizaNoEndereco = realizaNoEndereco,
            Ordem = ordem
        };
    }

    private static string? NormalizarCnae(string? cnae)
    {
        if (string.IsNullOrWhiteSpace(cnae)) return null;

        var normalizado = new string(cnae.Where(char.IsDigit).ToArray());
        if (normalizado.Length is < 4 or > 7)
            throw new ArgumentException("CNAE deve conter entre 4 e 7 digitos.", nameof(cnae));

        return normalizado;
    }
}
