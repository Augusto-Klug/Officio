using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RevogarLinkFichaEmpresaAdmissao;

public record RevogarLinkFichaEmpresaAdmissaoComando(Guid AdmissaoId)
    : IRequest<Resultado<bool>>;
