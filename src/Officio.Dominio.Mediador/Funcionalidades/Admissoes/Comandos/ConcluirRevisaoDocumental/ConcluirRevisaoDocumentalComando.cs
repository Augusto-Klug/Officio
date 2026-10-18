using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirRevisaoDocumental;

public record ConcluirRevisaoDocumentalComando(Guid AdmissaoId) : IRequest<Resultado<bool>>;
