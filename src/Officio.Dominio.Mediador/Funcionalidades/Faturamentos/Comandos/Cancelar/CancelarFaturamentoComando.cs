using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.Cancelar;

public record CancelarFaturamentoComando(Guid Id) : IRequest<Resultado<bool>>;
