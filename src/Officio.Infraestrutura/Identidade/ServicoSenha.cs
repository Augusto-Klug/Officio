using Microsoft.AspNetCore.Identity;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Identidade;

public class ServicoSenha : IServicoSenha
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public string GerarHash(Usuario usuario, string senha)
        => _passwordHasher.HashPassword(usuario, senha);
}
