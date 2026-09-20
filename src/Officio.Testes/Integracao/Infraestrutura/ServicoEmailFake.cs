using Officio.Dominio.Interfaces;

namespace Officio.Testes.Integracao.Infraestrutura;

public sealed class ServicoEmailFake : IServicoEmail
{
    private readonly List<EmailEnviado> emails = [];

    public IReadOnlyList<EmailEnviado> Emails
    {
        get
        {
            lock (emails)
                return emails.ToList();
        }
    }

    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken ct = default)
    {
        lock (emails)
            emails.Add(new EmailEnviado(to, subject, body));

        return Task.CompletedTask;
    }

    public void Limpar()
    {
        lock (emails)
            emails.Clear();
    }
}

public sealed record EmailEnviado(string Destinatario, string Assunto, string Corpo);
