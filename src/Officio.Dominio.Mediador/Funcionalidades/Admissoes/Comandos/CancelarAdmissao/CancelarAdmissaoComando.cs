using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CancelarAdmissao;

public record CancelarAdmissaoComando(Guid AdmissaoId) : IRequest<Resultado<bool>>;
