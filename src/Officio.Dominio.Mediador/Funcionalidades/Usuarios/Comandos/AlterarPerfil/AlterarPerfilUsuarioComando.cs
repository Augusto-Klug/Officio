using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.AlterarPerfil;

public record AlterarPerfilUsuarioComando(Guid UsuarioId, string Perfil) : IRequest<Resultado<bool>>;
