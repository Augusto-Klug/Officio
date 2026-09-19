namespace Officio.Dominio.Admissoes;

public static class StatusAdmissaoCodigos
{
    public const string Rascunho = "RASCUNHO";
    public const string AguardandoPreenchimento = "AGUARDANDO_PREENCHIMENTO";
    public const string EmValidacao = "EM_VALIDACAO";
    public const string PendenteRevisao = "PENDENTE_REVISAO";
    public const string Concluida = "CONCLUIDA";
    public const string Cancelada = "CANCELADA";

    public static bool Terminal(string codigo)
        => codigo is Concluida or Cancelada;
}
