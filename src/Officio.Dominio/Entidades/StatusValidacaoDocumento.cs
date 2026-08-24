namespace Officio.Dominio.Entidades;

public class StatusValidacaoDocumento : EntidadeDominio
{
    private StatusValidacaoDocumento() { }

    public StatusValidacaoDocumento(string codigo, string descricao)
        : base(codigo, descricao) { }
}
