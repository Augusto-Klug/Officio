namespace Officio.Dominio.Contratos;

public static class OrcamentoErros
{
    public const string NaoEncontrado = "Orcamento nao encontrado.";
    public const string SomenteRascunhoPodeSerExcluido = "Somente orcamentos em rascunho podem ser excluidos.";
    public const string AceiteRequerConfirmacao = "Ja existe um orcamento aceito para este prospecto. Confirme a substituicao para continuar.";
    public const string AceiteConcorrente = "O orcamento aceito deste prospecto foi alterado durante a operacao. Atualize a lista e tente novamente.";
    public const string SubstituicaoBloqueadaContrato = "O orcamento aceito atual ja esta vinculado a um contrato e nao pode ser substituido por este fluxo comercial.";
    public const string HonorarioMensalDuplicado = "O orcamento ja possui um item com natureza Honorario mensal. Inclua no maximo um item dessa natureza.";
    public const string PorFolhaDuplicado = "O orcamento ja possui um item com natureza Por folha. Inclua no maximo um item dessa natureza.";
}
