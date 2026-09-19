using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.AlterarStatus;

public record AlterarStatusUsuarioComando(Guid UsuarioId, bool Ativo) : IRequest<Resultado<bool>>;
