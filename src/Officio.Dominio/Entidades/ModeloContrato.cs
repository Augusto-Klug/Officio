namespace Officio.Dominio.Entidades;

public class ModeloContrato : EntidadeDominio
{
    public string ConteudoModelo { get; private set; } = default!;
    public int VersaoAtual { get; private set; } = 1;

    private ModeloContrato() { }

    public ModeloContrato(string codigo, string descricao, string conteudoModelo)
        : base(codigo, descricao)
        => ConteudoModelo = conteudoModelo;
}
