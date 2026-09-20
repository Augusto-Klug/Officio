using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AprovarDocumentoAdmissao;

public record AprovarDocumentoAdmissaoComando(AprovarDocumentoAdmissaoRequisicao Requisicao)
    : IRequest<Resultado<bool>>;
