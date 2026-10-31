namespace Officio.Dominio.Contratos;

public static class ProspectoErros
{
    public const string NaoEncontrado = "Prospecto nao encontrado.";
    public const string ForaDaEtapaInicial = "Somente prospectos em negociacao, ainda sem conversao ou empresa vinculada, podem ser editados ou excluidos.";
    public const string CadastroBloqueado = "O cadastro nao pode ser alterado nesta etapa ou porque ja possui empresa ou cliente vinculado.";
    public const string ContratoAguardandoAssinatura = "Nao e possivel alterar dados enquanto houver contrato aguardando assinatura.";
    public const string ConfirmacaoCancelamentoNecessaria = "Confirme o cancelamento dos contratos antes de alterar o prospecto.";
    public const string PossuiVinculos = "O prospecto possui orcamentos, contratos ou outros registros vinculados e nao pode ser excluido.";
}
