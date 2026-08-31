namespace Officio.Dominio.Interfaces;

public interface IServicoEmail
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}





