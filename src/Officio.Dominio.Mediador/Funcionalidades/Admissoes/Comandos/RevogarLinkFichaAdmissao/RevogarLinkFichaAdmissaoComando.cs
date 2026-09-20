using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RevogarLinkFichaAdmissao;

public record RevogarLinkFichaAdmissaoComando(Guid AdmissaoId) : IRequest<Resultado<bool>>;
