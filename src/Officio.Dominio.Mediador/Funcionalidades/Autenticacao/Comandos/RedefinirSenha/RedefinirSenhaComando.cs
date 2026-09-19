using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.RedefinirSenha;

public record RedefinirSenhaComando(string Token, string Senha) : IRequest<Resultado<bool>>;
