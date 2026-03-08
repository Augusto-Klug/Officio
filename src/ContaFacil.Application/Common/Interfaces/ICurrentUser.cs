namespace ContaFacil.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string? Email { get; }
}
