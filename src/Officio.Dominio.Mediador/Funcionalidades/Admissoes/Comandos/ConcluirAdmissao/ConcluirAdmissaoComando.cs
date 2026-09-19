using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirAdmissao;

public record ConcluirAdmissaoComando(Guid AdmissaoId) : IRequest<Resultado<bool>>;
