namespace Officio.Dominio.Servicos.Servicos;

internal static class TemplatesEmailAcesso
{
    public static (string Assunto, string Corpo) Convite(string nome, string link, double validadeHoras)
        => (
            "Convite para acessar o Officio",
            $"Ola, {nome}.\n\n" +
            "Voce foi convidado para acessar o Officio. Use o link abaixo para definir sua senha e ativar sua conta.\n" +
            $"Este convite expira em {FormatarDuracao(validadeHoras)}. Se voce nao esperava este convite, ignore esta mensagem.\n\n" +
            link);

    public static (string Assunto, string Corpo) RedefinicaoSenha(string link, double validadeHoras)
        => (
            "Redefinicao de senha do Officio",
            "Recebemos uma solicitacao para redefinir a senha da sua conta. Use o link abaixo para criar uma nova senha.\n" +
            $"O link expira em {FormatarDuracao(validadeHoras)} e pode ser usado uma unica vez. Se voce nao solicitou a redefinicao, ignore esta mensagem.\n\n" +
            link);

    private static string FormatarDuracao(double horas)
        => horas == 1 ? "1 hora" : $"{horas:0.##} horas";
}
