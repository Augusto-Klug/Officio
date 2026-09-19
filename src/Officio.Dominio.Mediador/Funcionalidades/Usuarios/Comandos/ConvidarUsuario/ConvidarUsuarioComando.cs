using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.ConvidarUsuario;

public record ConvidarUsuarioComando(string Nome, string Email, string Perfil)
    : IRequest<Resultado<ConviteCriadoDto>>;
