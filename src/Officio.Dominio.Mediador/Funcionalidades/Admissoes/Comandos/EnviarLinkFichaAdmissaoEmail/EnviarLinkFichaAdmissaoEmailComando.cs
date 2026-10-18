using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.EnviarLinkFichaAdmissaoEmail;

public record EnviarLinkFichaAdmissaoEmailComando(EnviarLinkFichaAdmissaoEmailRequisicao Requisicao)
    : IRequest<Resultado<bool>>;
