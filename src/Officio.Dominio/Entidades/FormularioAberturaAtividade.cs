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

    public static FormularioAberturaAtividade Criar(Guid formularioAberturaId, string descricaoAtividade, string? cnae = null)
        => new() { FormularioAberturaId = formularioAberturaId, DescricaoAtividade = descricaoAtividade, Cnae = cnae };
}
