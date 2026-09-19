using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IServicoSenha
{
    string GerarHash(Usuario usuario, string senha);
}
