using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.Fechar;

public record FecharFaturamentoComando(Guid Id) : IRequest<Resultado<bool>>;
