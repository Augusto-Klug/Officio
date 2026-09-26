using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.AlterarStatusParceiro;

public record AlterarStatusParceiroComando(Guid ParceiroId, bool Ativo) : IRequest<Resultado<bool>>;
