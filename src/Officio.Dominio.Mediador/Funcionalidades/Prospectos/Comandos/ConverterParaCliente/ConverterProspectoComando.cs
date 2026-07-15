using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;

public record ConverterProspectoComando(Guid ProspectId, string? SciCodigo = null) : IRequest<Resultado<Guid>>;





