using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.GerarLinkFichaAdmissao;

public record GerarLinkFichaAdmissaoComando(Guid AdmissaoId)
    : IRequest<Resultado<GerarLinkFichaAdmissaoDto>>;
