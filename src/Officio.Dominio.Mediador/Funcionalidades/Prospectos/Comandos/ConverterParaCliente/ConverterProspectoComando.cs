using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;

public record ConverterProspectoComando(Guid ProspectId, string? SciCodigo = null) : IRequest<Resultado<Guid>>;





