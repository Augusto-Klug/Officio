using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.AceitarConvite;

public record AceitarConviteComando(string Token, string Senha) : IRequest<Resultado<bool>>;
