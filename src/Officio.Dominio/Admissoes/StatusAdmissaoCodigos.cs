namespace Officio.Dominio.Admissoes;

public static class StatusAdmissaoCodigos
{
    public const string Rascunho = "RASCUNHO";
    public const string AguardandoAdmitido = "AGUARDANDO_ADMITIDO";
    public const string AguardandoValidacao = "AGUARDANDO_VALIDACAO";
    public const string DocumentosValidados = "DOCUMENTOS_VALIDADOS";
    public const string AguardandoEmpregador = "AGUARDANDO_EMPREGADOR";
    public const string LiberadoPeloEmpregador = "LIBERADO_PELO_EMPREGADOR";
    public const string LiberadoParaRevisaoFinal = "LIBERADO_PARA_REVISAO_FINAL";
    public const string Concluida = "CONCLUIDA";
    public const string Cancelada = "CANCELADA";

    public static bool Terminal(string codigo)
        => codigo is Concluida or Cancelada;
}
