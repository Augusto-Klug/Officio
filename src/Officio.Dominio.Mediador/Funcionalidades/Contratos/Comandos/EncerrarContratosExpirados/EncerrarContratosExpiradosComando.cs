using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EncerrarContratosExpirados;

public sealed record EncerrarContratosExpiradosComando(DateOnly DataReferencia)
    : IRequest<Resultado<int>>;
