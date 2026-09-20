using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.EnviarDocumentoAdmissao;

public record EnviarDocumentoAdmissaoComando(EnviarDocumentoAdmissaoRequisicao Requisicao)
    : IRequest<Resultado<ResultadoDocumentoAdmissaoDto>>;
