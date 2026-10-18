using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.GerarLinkFichaEmpresaAdmissao;

public record GerarLinkFichaEmpresaAdmissaoComando(Guid AdmissaoId)
    : IRequest<Resultado<GerarLinkFichaAdmissaoDto>>;
