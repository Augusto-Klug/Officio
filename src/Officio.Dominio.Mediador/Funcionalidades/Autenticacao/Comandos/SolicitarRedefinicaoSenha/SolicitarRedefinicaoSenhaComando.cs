using MediatR;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.SolicitarRedefinicaoSenha;

public record SolicitarRedefinicaoSenhaComando(string Email) : IRequest;
